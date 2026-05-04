using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterData : MonoBehaviour
{
    // ------------------------------------------------------------------ //
    //  Identificación
    // ------------------------------------------------------------------ //
    [Header("Identificación")]
    public string characterName;

    // ------------------------------------------------------------------ //
    //  FMOD
    // ------------------------------------------------------------------ //
    [Header("FMOD")]
    public string fmodParameter; // "stability_aura", "stability_alberto", etc.

    // ------------------------------------------------------------------ //
    //  Estabilidad  (todos los valores son ajustables en el Inspector)
    // ------------------------------------------------------------------ //
    [Header("Estabilidad")]
    [Range(0, 100)] public float stability = 100f;
    public float passiveDrain  = 0f;   // drain constante (Ruby usa 1.5)
    public float eventDrain    = 12f;  // drain extra mientras el evento está activo

    // ------------------------------------------------------------------ //
    //  UI
    // ------------------------------------------------------------------ //
    [Header("UI")]
    public Slider           stabilitySlider;   // slider de estabilidad
    public Image            fillImage;         // Image del "Fill" del slider (cambia de color)
    public TextMeshProUGUI  feelingText;        // texto que muestra el estado ("Tenso", etc.)

    // ------------------------------------------------------------------ //
    //  Colores por umbral
    // ------------------------------------------------------------------ //
    static readonly Color ColPositivo      = new Color(0.18f, 0.76f, 0.34f);
    static readonly Color ColTenso         = new Color(0.93f, 0.75f, 0.10f);
    static readonly Color ColDesestabilizado = new Color(0.93f, 0.45f, 0.10f);
    static readonly Color ColCritico       = new Color(0.85f, 0.18f, 0.18f);
    static readonly Color ColFallado       = new Color(0.35f, 0.35f, 0.35f);

    // ------------------------------------------------------------------ //
    //  Estado público (solo lectura desde fuera)
    // ------------------------------------------------------------------ //
    public enum EstadoEstabilidad { Positivo, Tenso, Desestabilizado, Critico, Fallado }
    public EstadoEstabilidad Estado { get; private set; }
    public float Stability => stability;
    public bool EventoActivo { get; private set; }
    public bool HasReachedCritical { get; private set; }

    // ------------------------------------------------------------------ //
    //  Estado interno
    // ------------------------------------------------------------------ //
    private bool _juegoActivo;
    private bool _pausado;
    private bool _failedNotificado;

    // ------------------------------------------------------------------ //
    //  Unity lifecycle
    // ------------------------------------------------------------------ //
    private void Start()
    {
        Estado = (EstadoEstabilidad)(-1); // fuerza el refresco visual inicial
        UpdateUI();
        PushToFMOD();
    }

    private void Update()
    {
        if (!_juegoActivo || _pausado || stability <= 0f) return;
        AplicarDrain();
        UpdateUI();
        PushToFMOD();
    }

    // ------------------------------------------------------------------ //
    //  API pública
    // ------------------------------------------------------------------ //

    /// <summary>Llamar desde MinigameManager al iniciar la partida.</summary>
    public void IniciarJuego()
    {
        _juegoActivo       = true;
        _failedNotificado  = false;
        HasReachedCritical = false;
        UpdateUI();
        PushToFMOD();
    }

    public void SetPausado(bool pausado)
    {
        _pausado = pausado;
    }

    public void ActivarEvento()
    {
        EventoActivo = true;
    }

    public void DesactivarEvento()
    {
        EventoActivo = false;
    }

    /// <summary>Sube estabilidad (tras completar un minijuego).</summary>
    public void AñadirEstabilidad(float cantidad)
    {
        bool eraZero = stability <= 0f;
        stability = Mathf.Clamp(stability + cantidad, 0f, 100f);

        if (eraZero && stability > 0f)
            MinigameManager.Instance.OnEstabilidadRecuperada(this);

        UpdateUI();
        PushToFMOD();
    }

    // ------------------------------------------------------------------ //
    //  Lógica interna
    // ------------------------------------------------------------------ //

    private void AplicarDrain()
    {
        float drain = passiveDrain + (EventoActivo ? eventDrain : 0f);
        stability = Mathf.Clamp(stability - drain * Time.deltaTime, 0f, 100f);

        if (stability <= 0f && !_failedNotificado)
        {
            _failedNotificado = true;
            MinigameManager.Instance.OnPersonajeFallado(this);
        }
    }

    private void UpdateUI()
    {
        if (stabilitySlider != null)
            stabilitySlider.value = stability;

        // Determinar estado
        EstadoEstabilidad nuevoEstado;
        if      (stability <= 0f)  nuevoEstado = EstadoEstabilidad.Fallado;
        else if (stability < 25f)  nuevoEstado = EstadoEstabilidad.Critico;
        else if (stability < 50f)  nuevoEstado = EstadoEstabilidad.Desestabilizado;
        else if (stability < 80f)  nuevoEstado = EstadoEstabilidad.Tenso;
        else                       nuevoEstado = EstadoEstabilidad.Positivo;

        // Primera vez en crítico → penalización de puntos
        if (nuevoEstado == EstadoEstabilidad.Critico && !HasReachedCritical)
        {
            HasReachedCritical = true;
            MinigameManager.Instance.OnPersonajeCritico();
        }

        if (nuevoEstado != Estado)
        {
            Estado = nuevoEstado;
            AplicarVisualesEstado();
        }
    }

    private void AplicarVisualesEstado()
    {
        Color color = Estado switch
        {
            EstadoEstabilidad.Positivo        => ColPositivo,
            EstadoEstabilidad.Tenso           => ColTenso,
            EstadoEstabilidad.Desestabilizado => ColDesestabilizado,
            EstadoEstabilidad.Critico         => ColCritico,
            EstadoEstabilidad.Fallado         => ColFallado,
            _                                 => ColPositivo
        };

        string textoEstado = Estado switch
        {
            EstadoEstabilidad.Positivo        => "Estable",
            EstadoEstabilidad.Tenso           => "Tenso",
            EstadoEstabilidad.Desestabilizado => "Desestabilizado",
            EstadoEstabilidad.Critico         => "Crítico",
            EstadoEstabilidad.Fallado         => "Fallado",
            _                                 => ""
        };

        if (fillImage  != null) fillImage.color  = color;
        if (feelingText != null) feelingText.text = textoEstado;
    }

    private void PushToFMOD()
    {
        if (string.IsNullOrEmpty(fmodParameter)) return;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(fmodParameter, stability);
    }
}
