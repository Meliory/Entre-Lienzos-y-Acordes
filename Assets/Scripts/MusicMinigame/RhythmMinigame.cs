using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RhythmMinigame : MonoBehaviour
{
    // ------------------------------------------------------------------ //
    //  UI
    // ------------------------------------------------------------------ //
    [Header("UI — Estructura")]
    public GameObject panel;

    [Header("UI — Elementos")]
    public Image           beatCircle;
    public Image[]         dots;           // tamaño 4: Dot1, Dot2, Dot3, Dot4
    public TextMeshProUGUI feedbackText;
    public Button          tapButton;

    [Header("UI — Resultado")]
    public GameObject      resultPanel;
    public TextMeshProUGUI resultTitle;
    public TextMeshProUGUI resultAccuracy;

    // ------------------------------------------------------------------ //
    //  Configuración
    // ------------------------------------------------------------------ //
    [Header("Configuración")]
    [Range(0f, 0.5f)] public float fraccionPerfecto  = 0.25f; // fracción del intervalo = ventana perfecta
    [Range(0f, 0.5f)] public float fraccionAceptable = 0.40f; // fracción del intervalo = ventana aceptable
    public int   totalTaps        = 4;
    public float escalaMaxPulso   = 1.50f;
    public float duracionPulso    = 0.12f;

    // ------------------------------------------------------------------ //
    //  Colores
    // ------------------------------------------------------------------ //
    static readonly Color ColCircleBase    = new Color(0.20f, 0.28f, 0.52f);
    static readonly Color ColCircleBeat    = new Color(0.40f, 0.60f, 1.00f);
    static readonly Color ColCirclePerfect = new Color(0.55f, 0.85f, 1.00f);
    static readonly Color ColCircleFail    = new Color(0.65f, 0.13f, 0.13f);

    static readonly Color ColDotDefault    = new Color(0.20f, 0.20f, 0.20f);
    static readonly Color ColDotWave       = new Color(0.35f, 0.45f, 0.75f);  // ola de espera
    static readonly Color ColDotActive     = new Color(0.55f, 0.70f, 1.00f);  // beat activo
    static readonly Color ColDotHit        = new Color(0.30f, 0.52f, 0.90f);
    static readonly Color ColDotFail       = new Color(0.65f, 0.13f, 0.13f);

    static readonly Color ColFeedbackPerfect    = new Color(0.38f, 0.72f, 0.38f);
    static readonly Color ColFeedbackAcceptable = new Color(0.82f, 0.70f, 0.22f);
    static readonly Color ColFeedbackFail       = new Color(0.75f, 0.22f, 0.22f);
    static readonly Color ColInstruccion        = new Color(0.60f, 0.60f, 0.60f);

    // ------------------------------------------------------------------ //
    //  Estado interno
    // ------------------------------------------------------------------ //
    private CharacterData                         _personaje;
    private Action<CharacterData, MinigameResult> _onCompletado;

    private bool  _activo;
    private bool  _pausado;
    private bool  _faseObservacion;   // true = esperando primer tap
    private bool  _aceptandoTaps;

    private float _bpm;
    private float _beatInterval;
    private float _beatStartTime;
    private int   _tapCount;
    private int   _tapsPerfectos;
    private int   _tapsAceptables;

    private Vector3   _escalaOriginalCircle;
    private Vector3   _escalaOriginalButton;
    private Coroutine _feedbackCoroutine;
    private Coroutine _colorCoroutine;
    private bool      _beatFlag;
    private float     _lastFlashBeatTime = -1f;

    // ================================================================== //
    //  Lifecycle
    // ================================================================== //

    private void OnEnable()
    {
        // Guardamos escala aquí para evitar el bug de Awake en GO desactivado
        if (_escalaOriginalCircle == Vector3.zero && beatCircle != null)
            _escalaOriginalCircle = beatCircle.transform.localScale;
        if (_escalaOriginalButton == Vector3.zero && tapButton != null)
            _escalaOriginalButton = tapButton.transform.localScale;

        tapButton?.onClick.AddListener(OnTap);
    }

    private void OnDisable() => tapButton?.onClick.RemoveListener(OnTap);

    // ================================================================== //
    //  API pública
    // ================================================================== //

    public void Activate(CharacterData personaje, float bpm, float beatStartTime, Action<CharacterData, MinigameResult> onCompletado)
    {
        if (_activo) return;

        _personaje      = personaje;
        _onCompletado   = onCompletado;
        _bpm            = bpm;
        _beatInterval   = 60f / _bpm;
        _tapCount       = 0;
        _tapsPerfectos  = 0;
        _tapsAceptables = 0;
        _activo         = true;
        _pausado        = false;
        _faseObservacion = true;
        _aceptandoTaps   = false;

        if (panel) panel.SetActive(true);
        ResetVisuales();

        _beatStartTime = beatStartTime;
        _beatFlag      = false;
        MinigameManager.OnBeat += OnBeatRecibido;

        StartCoroutine(RutinaObservacion());
    }

    public bool IsActivo => _activo;

    public void SetPausado(bool pausado) => _pausado = pausado;

    private void OnBeatRecibido(float bpm)
    {
        _beatStartTime = Time.time;
        _beatInterval  = 60f / bpm;
        _beatFlag      = true;
    }

    // ================================================================== //
    //  Fase de observación — el jugador ve el patrón antes de actuar
    // ================================================================== //

    private IEnumerator RutinaObservacion()
    {
        SetFeedback("Sigue el pulso — TAP cuando estés listo", ColInstruccion);

        int dotWaveIndex = 0;

        while (_faseObservacion && _activo)
        {
            // Esperar el siguiente beat real de FMOD
            _beatFlag = false;
            while (!_beatFlag && _faseObservacion && _activo) yield return null;
            if (!_faseObservacion || !_activo) yield break;

            while (_pausado) yield return null;

            _lastFlashBeatTime = Time.time;
            FlashCirculo(ColCircleBeat, 0.10f);
            StartCoroutine(PulsoEscala(escalaMaxPulso));
            StartCoroutine(PulsarDot(dotWaveIndex % dots.Length, ColDotWave));
            dotWaveIndex++;
        }
    }

    private IEnumerator PulsarDot(int index, Color color)
    {
        if (index >= dots.Length || dots[index] == null) yield break;
        // Solo pulsar si el dot no ha sido usado todavía
        if (_tapCount > index) yield break;

        dots[index].color = color;
        yield return new WaitForSeconds(0.12f);
        if (_tapCount <= index && dots[index] != null)
            dots[index].color = ColDotDefault;
    }

    // ================================================================== //
    //  Fase activa — después del primer tap
    // ================================================================== //

    private IEnumerator RutinaTaps()
    {
        for (int beat = _tapCount; beat < totalTaps; beat++)
        {
            _beatFlag = false;
            while (!_beatFlag && _activo) yield return null;
            if (!_activo) yield break;

            while (_pausado) yield return null;

            if (_tapCount == beat)
            {
                _lastFlashBeatTime = Time.time;
                FlashCirculo(ColCircleBeat, 0.10f);
                StartCoroutine(PulsoEscala(escalaMaxPulso));
                StartCoroutine(ParpadearDotActivo(beat));
            }
        }

        // Beat extra para dar tiempo al último tap
        _beatFlag = false;
        while (!_beatFlag && _activo) yield return null;

        while (_tapCount < totalTaps)
        {
            if (_tapCount < dots.Length && dots[_tapCount] != null)
                dots[_tapCount].color = ColDotFail;
            _tapCount++;
        }

        _aceptandoTaps = false;
        StartCoroutine(Finalizar());
    }

    private IEnumerator ParpadearDotActivo(int index)
    {
        if (index >= dots.Length || dots[index] == null) yield break;
        float duracion = _beatInterval * 0.9f;
        float t = 0f;
        bool encendido = true;

        while (t < duracion && _tapCount <= index && _activo)
        {
            while (_pausado) yield return null;
            dots[index].color = encendido ? ColDotActive : ColDotDefault;
            encendido = !encendido;
            float intervalo = _beatInterval * 0.25f;
            yield return new WaitForSeconds(intervalo);
            t += intervalo;
        }
    }

    // ================================================================== //
    //  Animaciones del círculo
    // ================================================================== //

    private void FlashCirculo(Color flash, float hold)
    {
        if (_colorCoroutine != null) StopCoroutine(_colorCoroutine);
        _colorCoroutine = StartCoroutine(FlashColor(flash, hold));
    }

    private IEnumerator FlashColor(Color flash, float holdTime)
    {
        if (beatCircle == null) yield break;
        beatCircle.color = flash;
        yield return new WaitForSeconds(holdTime);

        float elapsed = 0f, fade = 0.20f;
        while (elapsed < fade)
        {
            if (!_pausado)
            {
                beatCircle.color = Color.Lerp(flash, ColCircleBase, elapsed / fade);
                elapsed += Time.deltaTime;
            }
            yield return null;
        }
        if (beatCircle) beatCircle.color = ColCircleBase;
    }

    private IEnumerator PulsoEscala(float multiplicador)
    {
        if (beatCircle == null) yield break;
        float elapsed = 0f;
        Vector3 max = _escalaOriginalCircle * multiplicador;

        while (elapsed < duracionPulso)
        {
            if (!_pausado)
            {
                float t = elapsed / duracionPulso;
                beatCircle.transform.localScale = Vector3.Lerp(_escalaOriginalCircle, max, Mathf.Sin(t * Mathf.PI));
                elapsed += Time.deltaTime;
            }
            yield return null;
        }
        if (beatCircle) beatCircle.transform.localScale = _escalaOriginalCircle;
    }

    // ================================================================== //
    //  Tap del jugador
    // ================================================================== //

    private void OnTap()
    {
        if (!_activo || _pausado) return;

        // Primer tap: salir de observación y entrar en fase activa
        if (_faseObservacion)
        {
            _faseObservacion = false;
            _aceptandoTaps   = true;
            SetFeedback("", ColInstruccion);

            // Medimos tap #1 contra el beat más cercano
            MedirTap();

            // Lanzar la rutina de beats siguientes
            StartCoroutine(RutinaTaps());
            return;
        }

        if (!_aceptandoTaps || _tapCount >= totalTaps) return;
        MedirTap();
    }

    private void MedirTap()
    {
        // Distancia al flash visual más reciente (o al siguiente esperado)
        float distancia = float.MaxValue;
        if (_lastFlashBeatTime >= 0f)
        {
            float despues = Time.time - _lastFlashBeatTime;                         // tiempo tras el flash
            float antes   = (_lastFlashBeatTime + _beatInterval) - Time.time;       // tiempo hasta el próximo
            distancia = Mathf.Min(Mathf.Abs(despues), Mathf.Abs(antes));
        }

        float ventPerfecto  = _beatInterval * fraccionPerfecto;
        float ventAceptable = _beatInterval * fraccionAceptable;

        string texto; Color colTexto, colDot, colFlash;

        if (distancia <= ventPerfecto)
        {
            _tapsPerfectos++;
            texto = "PERFECTO"; colTexto = ColFeedbackPerfect;
            colDot = ColDotHit; colFlash = ColCirclePerfect;
        }
        else if (distancia <= ventAceptable)
        {
            _tapsAceptables++;
            texto = "BIEN";     colTexto = ColFeedbackAcceptable;
            colDot = ColDotHit; colFlash = ColCircleBeat;
        }
        else
        {
            texto = "FALLADO";  colTexto = ColFeedbackFail;
            colDot = ColDotFail; colFlash = ColCircleFail;
        }

        if (_tapCount < dots.Length && dots[_tapCount] != null)
            dots[_tapCount].color = colDot;

        FlashCirculo(colFlash, 0.15f);

        if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
        _feedbackCoroutine = StartCoroutine(MostrarFeedback(texto, colTexto));

        StartCoroutine(AnimarBoton());
        _tapCount++;
    }

    private IEnumerator MostrarFeedback(string texto, Color color)
    {
        if (feedbackText == null) yield break;
        feedbackText.text  = texto;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        if (_faseObservacion) yield break; // en observación no ocultamos la instrucción
        feedbackText.gameObject.SetActive(false);
    }

    private void SetFeedback(string texto, Color color)
    {
        if (feedbackText == null) return;
        feedbackText.text  = texto;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(texto.Length > 0);
    }

    private IEnumerator AnimarBoton()
    {
        if (tapButton == null) yield break;
        tapButton.transform.localScale = _escalaOriginalButton * 0.90f;
        yield return new WaitForSeconds(0.08f);
        tapButton.transform.localScale = _escalaOriginalButton;
    }

    // ================================================================== //
    //  Finalización
    // ================================================================== //

    private IEnumerator Finalizar()
    {
        yield return new WaitForSeconds(0.3f);

        int aciertos = _tapsPerfectos + _tapsAceptables;
        float precision = (float)aciertos / totalTaps * 100f;

        MinigameResult resultado;
        string titulo;
        Color color;

        if (_tapsPerfectos >= 3)
        {
            resultado = MinigameResult.Perfect;
            titulo = "CONCENTRADO"; color = ColFeedbackPerfect;
        }
        else if (aciertos >= 2)
        {
            resultado = MinigameResult.Acceptable;
            titulo = "INESTABLE";   color = ColFeedbackAcceptable;
        }
        else
        {
            resultado = MinigameResult.Failed;
            titulo = "BLOQUEADO";   color = ColFeedbackFail;
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if (resultTitle)    { resultTitle.text = titulo; resultTitle.color = color; }
            if (resultAccuracy) resultAccuracy.text = $"Precision: {precision:F0}%";
        }

        yield return new WaitForSeconds(2f);
        Desactivar(resultado);
    }

    private void Desactivar(MinigameResult resultado)
    {
        _activo          = false;
        _faseObservacion = false;
        _aceptandoTaps   = false;
        MinigameManager.OnBeat -= OnBeatRecibido;
        StopAllCoroutines();
        if (beatCircle)
        {
            beatCircle.transform.localScale = _escalaOriginalCircle;
            beatCircle.color = ColCircleBase;
        }
        if (panel) panel.SetActive(false);
        _onCompletado?.Invoke(_personaje, resultado);
    }

    // ================================================================== //
    //  Reset
    // ================================================================== //

    private void ResetVisuales()
    {
        if (beatCircle)
        {
            beatCircle.color = ColCircleBase;
            beatCircle.transform.localScale = _escalaOriginalCircle;
        }
        if (dots != null)
            foreach (var d in dots) if (d != null) d.color = ColDotDefault;

        if (feedbackText)  feedbackText.gameObject.SetActive(false);
        if (resultPanel)   resultPanel.SetActive(false);
    }
}
