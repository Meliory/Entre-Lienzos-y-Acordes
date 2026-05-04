using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BreathingMinigame : MonoBehaviour
{
    // ------------------------------------------------------------------ //
    //  UI
    // ------------------------------------------------------------------ //
    [Header("UI — Estructura")]
    public GameObject panel;

    [Header("UI — Círculo")]
    public RectTransform breathCircle;
    public Image         breathCircleImage;

    [Header("UI — Ciclos (3 dots)")]
    public Image[] cycleDots;

    [Header("UI — Textos")]
    public TextMeshProUGUI instruccionText;

    [Header("UI — Botón")]
    public Button breathButton;

    [Header("UI — Resultado")]
    public GameObject      resultPanel;
    public TextMeshProUGUI resultTitle;
    public TextMeshProUGUI resultAccuracy;

    // ------------------------------------------------------------------ //
    //  Configuración
    // ------------------------------------------------------------------ //
    [Header("Configuración — Tiempos (aleatorios por ciclo)")]
    public float inhalacionMin = 1.2f;
    public float inhalacionMax = 2.2f;
    public float exhalacionMin = 1.2f;
    public float exhalacionMax = 2.0f;

    [Header("Configuración — Evaluación")]
    public int   totalCiclos  = 3;
    [Range(0f, 1f)]
    public float umbralCorrecto = 0.5f;

    [Header("Escala del círculo")]
    public float escalaMin = 0.4f;
    public float escalaMax = 1.6f;

    // ------------------------------------------------------------------ //
    //  Colores
    // ------------------------------------------------------------------ //
    static readonly Color ColCircleBase   = new Color(0.35f, 0.55f, 0.90f);
    static readonly Color ColCircleInhala = new Color(0.35f, 0.85f, 0.55f);
    static readonly Color ColCircleExhala = new Color(0.55f, 0.75f, 1.00f);
    static readonly Color ColCircleError  = new Color(0.75f, 0.22f, 0.22f);
    static readonly Color ColCirclePeak   = new Color(0.65f, 0.95f, 0.65f);

    static readonly Color ColDotPendiente = new Color(0.55f, 0.70f, 1.00f);
    static readonly Color ColDotCorrecto  = new Color(0.30f, 0.80f, 0.40f);
    static readonly Color ColDotFallo     = new Color(0.65f, 0.13f, 0.13f);

    static readonly Color ColInstruccion  = new Color(0.70f, 0.70f, 0.70f);
    static readonly Color ColInstruccionOk = new Color(0.35f, 0.85f, 0.55f);
    static readonly Color ColInstruccionEx = new Color(0.55f, 0.75f, 1.00f);

    static readonly Color ColResultOk   = new Color(0.38f, 0.72f, 0.38f);
    static readonly Color ColResultFail = new Color(0.75f, 0.22f, 0.22f);

    // ------------------------------------------------------------------ //
    //  Estado interno
    // ------------------------------------------------------------------ //
    private CharacterData                         _personaje;
    private Action<CharacterData, MinigameResult> _onCompletado;
    private bool _activo;
    private bool _pausado;
    private bool _botonPulsado;

    // ================================================================== //
    //  Awake — detección de pulsación prolongada
    // ================================================================== //
    private void Awake()
    {
        if (breathButton == null) return;
        var trigger = breathButton.gameObject.GetComponent<EventTrigger>()
                   ?? breathButton.gameObject.AddComponent<EventTrigger>();

        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener(_ => _botonPulsado = true);
        trigger.triggers.Add(down);

        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener(_ => _botonPulsado = false);
        trigger.triggers.Add(up);
    }

    // ================================================================== //
    //  API pública
    // ================================================================== //

    public void Activate(CharacterData personaje, float bpm, Action<CharacterData, MinigameResult> onCompletado)
    {
        if (_activo) return;

        _personaje    = personaje;
        _onCompletado = onCompletado;
        _activo       = true;
        _pausado      = false;
        _botonPulsado = false;

        ResetVisuales();
        if (panel) panel.SetActive(true);
        StartCoroutine(RutinaRespiracion());
    }

    public void SetPausado(bool pausado)
    {
        _pausado = pausado;
        if (pausado) _botonPulsado = false;
    }

    // ================================================================== //
    //  Rutina principal
    // ================================================================== //

    private IEnumerator RutinaRespiracion()
    {
        int ciclosCorrectos = 0;

        for (int ciclo = 0; ciclo < totalCiclos; ciclo++)
        {
            // ---------------------------------------------------------- //
            //  ESPERA: quieto hasta que el jugador pulse y mantenga
            // ---------------------------------------------------------- //
            SetColorCirculo(ColCircleBase);
            SetInstruccion("Inhala", ColInstruccion);
            if (breathCircle) breathCircle.localScale = Vector3.one * escalaMin;

            while (!_botonPulsado && _activo)
            {
                while (_pausado) yield return null;
                yield return null;
            }
            if (!_activo) yield break;

            // Duración aleatoria para este ciclo
            float durInhala = UnityEngine.Random.Range(inhalacionMin, inhalacionMax);
            float durExhala = UnityEngine.Random.Range(exhalacionMin, exhalacionMax);

            // ---------------------------------------------------------- //
            //  INHALACIÓN: jugador mantiene pulsado, círculo crece
            // ---------------------------------------------------------- //
            float tiempoOkInhala = 0f;
            float elapsed = 0f;
            SetInstruccion("Inhala", ColInstruccionOk);

            while (elapsed < durInhala)
            {
                while (_pausado) yield return null;

                bool correcto = _botonPulsado;
                if (correcto) tiempoOkInhala += Time.deltaTime;
                SetColorCirculo(correcto ? ColCircleInhala : ColCircleError);

                float t = elapsed / durInhala;
                if (breathCircle)
                    breathCircle.localScale = Vector3.one * Mathf.Lerp(escalaMin, escalaMax, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (breathCircle) breathCircle.localScale = Vector3.one * escalaMax;

            // ---------------------------------------------------------- //
            //  PICO: flash breve antes de exhalar
            // ---------------------------------------------------------- //
            SetColorCirculo(ColCirclePeak);
            SetInstruccion("Exhala", ColInstruccionEx);
            yield return new WaitForSeconds(0.15f);

            // ---------------------------------------------------------- //
            //  EXHALACIÓN: jugador suelta, círculo encoge
            // ---------------------------------------------------------- //
            float tiempoOkExhala = 0f;
            elapsed = 0f;

            while (elapsed < durExhala)
            {
                while (_pausado) yield return null;

                bool correcto = !_botonPulsado;
                if (correcto) tiempoOkExhala += Time.deltaTime;
                SetColorCirculo(correcto ? ColCircleExhala : ColCircleError);

                float t = elapsed / durExhala;
                if (breathCircle)
                    breathCircle.localScale = Vector3.one * Mathf.Lerp(escalaMax, escalaMin, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (breathCircle) breathCircle.localScale = Vector3.one * escalaMin;

            // ---------------------------------------------------------- //
            //  EVALUACIÓN del ciclo
            // ---------------------------------------------------------- //
            bool inhalacionOk = tiempoOkInhala >= durInhala * umbralCorrecto;
            bool exhalacionOk = tiempoOkExhala >= durExhala * umbralCorrecto;
            bool cicloOk      = inhalacionOk && exhalacionOk;

            if (cicloOk) ciclosCorrectos++;
            SetDot(ciclo, cicloOk ? ColDotCorrecto : ColDotFallo);

            if (ciclo < totalCiclos - 1)
                yield return new WaitForSeconds(0.25f);
        }

        // ---------------------------------------------------------------- //
        //  RESULTADO FINAL
        // ---------------------------------------------------------------- //
        MinigameResult resultado = ciclosCorrectos switch
        {
            3 => MinigameResult.Perfect,
            2 => MinigameResult.Acceptable,
            _ => MinigameResult.Failed
        };

        string titulo   = resultado == MinigameResult.Perfect    ? "EN CALMA"
                        : resultado == MinigameResult.Acceptable ? "CASI"
                        : "BLOQUEADA";
        Color colTitulo = resultado == MinigameResult.Failed ? ColResultFail : ColResultOk;

        SetColorCirculo(ColCircleBase);
        SetInstruccion("", ColInstruccion);

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if (resultTitle)    { resultTitle.text = titulo; resultTitle.color = colTitulo; }
            if (resultAccuracy) resultAccuracy.text = $"Ciclos correctos: {ciclosCorrectos}/{totalCiclos}";
        }

        yield return new WaitForSeconds(2f);
        Desactivar(resultado);
    }

    // ================================================================== //
    //  Desactivar
    // ================================================================== //

    private void Desactivar(MinigameResult resultado)
    {
        _activo       = false;
        _botonPulsado = false;
        StopAllCoroutines();
        if (breathCircle)      breathCircle.localScale = Vector3.one * escalaMin;
        if (breathCircleImage) breathCircleImage.color = ColCircleBase;
        if (panel)             panel.SetActive(false);
        _onCompletado?.Invoke(_personaje, resultado);
    }

    // ================================================================== //
    //  Helpers visuales
    // ================================================================== //

    private void ResetVisuales()
    {
        if (breathCircle)      breathCircle.localScale = Vector3.one * escalaMin;
        if (breathCircleImage) breathCircleImage.color = ColCircleBase;
        if (instruccionText)   instruccionText.gameObject.SetActive(false);
        if (resultPanel)       resultPanel.SetActive(false);
        if (cycleDots != null)
            foreach (var d in cycleDots) if (d) d.color = ColDotPendiente;
    }

    private void SetInstruccion(string texto, Color color)
    {
        if (instruccionText == null) return;
        instruccionText.text  = texto;
        instruccionText.color = color;
        instruccionText.gameObject.SetActive(texto.Length > 0);
    }

    private void SetColorCirculo(Color color)
    {
        if (breathCircleImage) breathCircleImage.color = color;
    }

    private void SetDot(int index, Color color)
    {
        if (cycleDots == null || index >= cycleDots.Length || cycleDots[index] == null) return;
        cycleDots[index].color = color;
    }
}
