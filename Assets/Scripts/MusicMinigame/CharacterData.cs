using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TipoMinijuego { Ninguno, Ritmo, Afinacion, Respiracion }

public class CharacterData : MonoBehaviour
{
    // ------------------------------------------------------------------ //
    //  Identificación
    // ------------------------------------------------------------------ //
    [Header("Identificación")]
    public string characterName;

    [Header("Tipo de Minijuego")]
    public TipoMinijuego tipoMinijuego = TipoMinijuego.Ninguno;

    // ------------------------------------------------------------------ //
    //  FMOD
    // ------------------------------------------------------------------ //
    [Header("FMOD — Estabilidad")]
    public string fmodParameter;

    [Header("FMOD — Distorsión")]
    public string fmodDistortionParameter;

    // ------------------------------------------------------------------ //
    //  Estabilidad
    // ------------------------------------------------------------------ //
    [Header("Estabilidad")]
    [Range(0, 100)] public float stability = 100f;
    public float passiveDrain = 0f;
    public float eventDrain   = 1.7f;

    // ------------------------------------------------------------------ //
    //  UI
    // ------------------------------------------------------------------ //
    [Header("UI")]
    public Slider          stabilitySlider;
    public Image           fillImage;
    public TextMeshProUGUI feelingText;

    // ------------------------------------------------------------------ //
    //  Burbuja
    // ------------------------------------------------------------------ //
    [Header("Burbuja")]
    public Button bubbleButton;

    // ------------------------------------------------------------------ //
    //  Colores por umbral
    // ------------------------------------------------------------------ //
    static readonly Color ColPositivo        = new Color(0.18f, 0.76f, 0.34f);
    static readonly Color ColTenso           = new Color(0.93f, 0.75f, 0.10f);
    static readonly Color ColDesestabilizado = new Color(0.93f, 0.45f, 0.10f);
    static readonly Color ColCritico         = new Color(0.85f, 0.18f, 0.18f);
    static readonly Color ColFallado         = new Color(0.35f, 0.35f, 0.35f);

    // ------------------------------------------------------------------ //
    //  Estado público
    // ------------------------------------------------------------------ //
    public enum EstadoEstabilidad { Positivo, Tenso, Desestabilizado, Critico, Fallado }
    public EstadoEstabilidad Estado          { get; private set; }
    public float             Stability       => stability;
    public bool              EventoActivo    { get; private set; }
    public bool              MinijuegoAbierto { get; private set; }
    public bool              HasReachedCritical { get; private set; }

    // ------------------------------------------------------------------ //
    //  Estado interno
    // ------------------------------------------------------------------ //
    private bool  _juegoActivo;
    private bool  _pausado;
    private bool  _failedNotificado;
    private float _distortionActual;

    // ------------------------------------------------------------------ //
    //  Unity lifecycle
    // ------------------------------------------------------------------ //
    private void Start()
    {
        Estado = (EstadoEstabilidad)(-1);
        OcultarBurbuja();
        bubbleButton?.onClick.AddListener(() => MinigameManager.Instance.OnBubbleClicked(this));
        UpdateUI();
        PushToFMOD();
    }

    private void Update()
    {
        if (!_juegoActivo || _pausado || stability <= 0f) return;
        AplicarDrain();
        UpdateUI();
        PushToFMOD();
        ActualizarDistorsion();
    }

    // ------------------------------------------------------------------ //
    //  API pública
    // ------------------------------------------------------------------ //

    public void IniciarJuego()
    {
        _juegoActivo       = true;
        _failedNotificado  = false;
        HasReachedCritical = false;
        UpdateUI();
        PushToFMOD();
    }

    public void SetPausado(bool pausado) => _pausado = pausado;

    public void ActivarEvento() => EventoActivo = true;

    public void DesactivarEvento()
    {
        EventoActivo     = false;
        MinijuegoAbierto = false;
        _distortionActual = 0f;
        if (!string.IsNullOrEmpty(fmodDistortionParameter))
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(fmodDistortionParameter, 0f);
    }

    public void AbrirMinijuego() => MinijuegoAbierto = true;

    public void MostrarBurbuja()
    {
        if (bubbleButton == null) return;
        bubbleButton.gameObject.SetActive(true);
        bubbleButton.interactable = true;
    }

    public void OcultarBurbuja() => bubbleButton?.gameObject.SetActive(false);

    public void SetBurbujaInteractuable(bool valor)
    {
        if (bubbleButton) bubbleButton.interactable = valor;
    }

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

    private void ActualizarDistorsion()
    {
        if (string.IsNullOrEmpty(fmodDistortionParameter)) return;

        float target = MinijuegoAbierto ? (1f - stability / 100f) : 0f;
        float speed  = MinijuegoAbierto ? 1.5f : 2f;
        _distortionActual = Mathf.MoveTowards(_distortionActual, target, speed * Time.deltaTime);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(fmodDistortionParameter, _distortionActual);
    }

    private void UpdateUI()
    {
        if (stabilitySlider != null)
            stabilitySlider.value = stability;

        EstadoEstabilidad nuevoEstado;
        if      (stability <= 0f)  nuevoEstado = EstadoEstabilidad.Fallado;
        else if (stability < 25f)  nuevoEstado = EstadoEstabilidad.Critico;
        else if (stability < 50f)  nuevoEstado = EstadoEstabilidad.Desestabilizado;
        else if (stability < 80f)  nuevoEstado = EstadoEstabilidad.Tenso;
        else                       nuevoEstado = EstadoEstabilidad.Positivo;

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

        if (fillImage   != null) fillImage.color  = color;
        if (feelingText != null) feelingText.text  = textoEstado;
    }

    private void PushToFMOD()
    {
        if (string.IsNullOrEmpty(fmodParameter)) return;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(fmodParameter, stability);
    }
}
