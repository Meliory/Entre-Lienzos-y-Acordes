using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TuningMinigame : MonoBehaviour
{
    // ------------------------------------------------------------------ //
    //  UI
    // ------------------------------------------------------------------ //
    [Header("UI — Estructura")]
    public GameObject panel;
    public Button     ajustarButton;

    [Header("UI — Medidor")]
    public RectTransform needle;
    public Image         needleImage;    // cambia de color al entrar en zona verde
    public RectTransform zonaVerdeRect;  // se usa para calcular semiAnchoZonaVerde automáticamente
    public RectTransform metroRect;      // contenedor del medidor (padre de ZonaVerde y Needle)

    [Header("UI — Intentos (3 dots)")]
    public Image[] attemptDots;          // Dot1, Dot2, Dot3

    [Header("UI — Textos")]
    public TextMeshProUGUI feedbackText;

    [Header("UI — Resultado")]
    public GameObject      resultPanel;
    public TextMeshProUGUI resultTitle;
    public TextMeshProUGUI resultAccuracy;

    // ------------------------------------------------------------------ //
    //  Configuración
    // ------------------------------------------------------------------ //
    [Header("Configuración")]
    public float velocidadInicial   = 55f;
    public float anguloMaximo       = 70f;
    public float semiAnchoZonaVerde = 12f;
    public float multiplicadorFallo = 1.7f;

    // ------------------------------------------------------------------ //
    //  Colores
    // ------------------------------------------------------------------ //
    static readonly Color ColNeedleBase   = Color.white;
    static readonly Color ColNeedleGreen  = new Color(0.35f, 0.85f, 0.45f);

    static readonly Color ColDotPendiente = new Color(0.55f, 0.70f, 1.00f);
    static readonly Color ColDotAcierto   = new Color(0.30f, 0.80f, 0.40f);
    static readonly Color ColDotFallo     = new Color(0.65f, 0.13f, 0.13f);

    static readonly Color ColFeedbackOk   = new Color(0.35f, 0.85f, 0.45f);
    static readonly Color ColFeedbackFail = new Color(0.75f, 0.22f, 0.22f);
    static readonly Color ColFeedbackHint = new Color(0.60f, 0.60f, 0.60f);

    static readonly Color ColResultOk     = new Color(0.38f, 0.72f, 0.38f);
    static readonly Color ColResultFail   = new Color(0.75f, 0.22f, 0.22f);

    // ------------------------------------------------------------------ //
    //  Estado interno
    // ------------------------------------------------------------------ //
    private CharacterData                         _personaje;
    private Action<CharacterData, MinigameResult> _onCompletado;
    private bool  _activo;
    private bool  _pausado;
    private bool  _enZonaVerde;

    private float _velocidadActual;
    private float _angulo;
    private float _direccion = 1f;
    private int   _intentosRestantes;
    private int   _aciertos;

    // ================================================================== //
    //  Lifecycle
    // ================================================================== //
    private void OnEnable()  => ajustarButton?.onClick.AddListener(OnAjustar);
    private void OnDisable() => ajustarButton?.onClick.RemoveListener(OnAjustar);

    // ================================================================== //
    //  API pública
    // ================================================================== //

    public void Activate(CharacterData personaje, float bpm, Action<CharacterData, MinigameResult> onCompletado)
    {
        if (_activo) return;

        _personaje    = personaje;
        _onCompletado = onCompletado;
        _activo  = true;
        _pausado = false;

        _velocidadActual   = velocidadInicial;
        _angulo            = 0f;
        _direccion         = 1f;
        _intentosRestantes = 3;
        _aciertos          = 0;

        ResetVisuales();
        if (panel) panel.SetActive(true);
        SetFeedback("Pulsa AJUSTAR en la zona verde", ColFeedbackHint);
        StartCoroutine(CalibrateZona());
    }

    public bool IsActivo => _activo;

    public void SetPausado(bool pausado) => _pausado = pausado;

    private IEnumerator CalibrateZona()
    {
        yield return null; // esperar un frame para que Unity calcule el layout
        if (needle == null || zonaVerdeRect == null) yield break;

        // Distancia desde el pivote hasta la punta de la aguja
        float tipDist = needle.rect.height * (1f - needle.pivot.y);
        if (tipDist <= 0f) yield break;

        // Ancho visual exacto donde la punta entra/sale de la zona verde
        float halfZone = tipDist * Mathf.Sin(semiAnchoZonaVerde * Mathf.Deg2Rad);
        zonaVerdeRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, halfZone * 2f);
    }

    // ================================================================== //
    //  Update — movimiento y color de la aguja
    // ================================================================== //
    private void Update()
    {
        if (!_activo || _pausado) return;

        _angulo += _direccion * _velocidadActual * Time.deltaTime;

        if      (_angulo >=  anguloMaximo) { _angulo =  anguloMaximo; _direccion = -1f; }
        else if (_angulo <= -anguloMaximo) { _angulo = -anguloMaximo; _direccion =  1f; }

        if (needle) needle.localRotation = Quaternion.Euler(0f, 0f, -_angulo);

        bool enZona = Mathf.Abs(_angulo) <= semiAnchoZonaVerde;
        if (enZona != _enZonaVerde)
        {
            _enZonaVerde = enZona;
            if (needleImage) needleImage.color = enZona ? ColNeedleGreen : ColNeedleBase;
        }
    }

    // ================================================================== //
    //  Botón AJUSTAR
    // ================================================================== //
    private void OnAjustar()
    {
        if (!_activo || _pausado || _intentosRestantes <= 0) return;

        int dotIndex = 3 - _intentosRestantes;

        if (_enZonaVerde)
        {
            _aciertos++;
            SetDot(dotIndex, ColDotAcierto);
            SetFeedback("¡En zona!", ColFeedbackOk);
        }
        else
        {
            SetDot(dotIndex, ColDotFallo);
            SetFeedback("Fuera de zona", ColFeedbackFail);
        }

        _velocidadActual *= multiplicadorFallo;
        _intentosRestantes--;

        if (_intentosRestantes <= 0)
            StartCoroutine(FinalizarTrasDelay());
    }

    // ================================================================== //
    //  Finalización
    // ================================================================== //
    private IEnumerator FinalizarTrasDelay()
    {
        yield return new WaitForSeconds(0.5f);

        MinigameResult resultado = _aciertos switch
        {
            3 => MinigameResult.Perfect,
            2 => MinigameResult.Acceptable,
            _ => MinigameResult.Failed
        };

        string titulo   = resultado == MinigameResult.Perfect    ? "AFINADA"
                        : resultado == MinigameResult.Acceptable ? "CASI"
                        : "DESAFINADA";
        Color colTitulo = resultado == MinigameResult.Failed ? ColResultFail : ColResultOk;

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if (resultTitle)    { resultTitle.text = titulo; resultTitle.color = colTitulo; }
            if (resultAccuracy) resultAccuracy.text = $"Aciertos: {_aciertos}/3";
        }

        yield return new WaitForSeconds(2f);
        Desactivar(resultado);
    }

    private void Desactivar(MinigameResult resultado)
    {
        _activo = false;
        StopAllCoroutines();
        if (needle)      needle.localRotation = Quaternion.identity;
        if (needleImage) needleImage.color = ColNeedleBase;
        if (panel)       panel.SetActive(false);
        _onCompletado?.Invoke(_personaje, resultado);
    }

    // ================================================================== //
    //  Helpers visuales
    // ================================================================== //
    private void ResetVisuales()
    {
        _enZonaVerde = false;
        if (needle)       needle.localRotation = Quaternion.identity;
        if (needleImage)  needleImage.color = ColNeedleBase;
        if (feedbackText) feedbackText.gameObject.SetActive(false);
        if (resultPanel)  resultPanel.SetActive(false);
        if (attemptDots != null)
            foreach (var d in attemptDots) if (d) d.color = ColDotPendiente;
    }

    private void SetFeedback(string texto, Color color)
    {
        if (feedbackText == null) return;
        feedbackText.text  = texto;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(texto.Length > 0);
    }

    private void SetDot(int index, Color color)
    {
        if (attemptDots == null || index >= attemptDots.Length || attemptDots[index] == null) return;
        attemptDots[index].color = color;
    }
}
