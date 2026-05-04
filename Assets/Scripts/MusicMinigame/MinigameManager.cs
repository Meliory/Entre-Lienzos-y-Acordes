using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }
    public static event Action<float> OnBeat;

    // ------------------------------------------------------------------ //
    //  Personajes
    // ------------------------------------------------------------------ //
    [Header("Personajes")]
    public CharacterData aura;
    public CharacterData alberto;
    public CharacterData ruby;
    public CharacterData ramon;

    // ------------------------------------------------------------------ //
    //  FMOD
    // ------------------------------------------------------------------ //
    [Header("FMOD")]
    public EventRef eventPath;

    // ------------------------------------------------------------------ //
    //  Minijuegos
    // ------------------------------------------------------------------ //
    [Header("Minijuegos")]
    public RhythmMinigame   rhythmMinigame;    // Alberto
    public TuningMinigame   tuningMinigame;    // Aura
    public BreathingMinigame breathingMinigame; // Ruby

    // ------------------------------------------------------------------ //
    //  BPM por sección  (asigna desde código al llamar cada evento)
    // ------------------------------------------------------------------ //
    [Header("BPM de la canción")]
    public float bpmPorDefecto = 160f;

    // ------------------------------------------------------------------ //
    //  UI — Puntuación
    // ------------------------------------------------------------------ //
    [Header("UI — Puntuación")]
    public TextMeshProUGUI scoreText;

    // ------------------------------------------------------------------ //
    //  UI — Pausa
    // ------------------------------------------------------------------ //
    [Header("UI — Pausa")]
    public GameObject      pausaCanvas;
    public TextMeshProUGUI cuentaAtrasText;

    // ------------------------------------------------------------------ //
    //  UI — Fin de partida
    // ------------------------------------------------------------------ //
    [Header("UI — Fin de partida")]
    public GameObject      gameOverCanvas;
    public GameObject      resultadosCanvas;
    public TextMeshProUGUI conclusionText;
    public TextMeshProUGUI puntuacionFinalText;

    // ------------------------------------------------------------------ //
    //  Estado interno
    // ------------------------------------------------------------------ //
    private int  _puntuacion;
    private bool _juegoActivo;
    private bool _pausado;
    private bool _partidaTerminada;

    private Coroutine    _gameOverTimer;
    private CharacterData _personajeFallado;

    // ------------------------------------------------------------------ //
    //  FMOD — instancia y callback
    // ------------------------------------------------------------------ //
    private FMOD.Studio.EventInstance _fmodInstance;
    private FMOD.Studio.EVENT_CALLBACK _fmodCallback;
    private static readonly ConcurrentQueue<string> _markerQueue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<float>  _beatQueue   = new ConcurrentQueue<float>();
    private float _lastBeatTime;
    private float _currentBpm;

    // ================================================================== //
    //  Unity lifecycle
    // ================================================================== //

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Ocultar todos los canvas de estado
        pausaCanvas?.SetActive(false);
        cuentaAtrasText?.gameObject.SetActive(false);
        gameOverCanvas?.SetActive(false);
        resultadosCanvas?.SetActive(false);

        // Iniciar FMOD
        _fmodInstance = FMODUnity.RuntimeManager.CreateInstance(eventPath);
        _fmodCallback = new FMOD.Studio.EVENT_CALLBACK(TimelineCallback);
        _fmodInstance.setCallback(_fmodCallback,
            FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER |
            FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

        IniciarPartida();
    }

    private void Update()
    {
        if (_partidaTerminada) return;

        // Sincronizar beats FMOD → Unity time
        while (_beatQueue.TryDequeue(out float tempo))
        {
            _lastBeatTime = Time.time;
            if (tempo > 0f) _currentBpm = tempo;
            OnBeat?.Invoke(_currentBpm);
        }

        // Procesar marcadores del hilo de audio en el hilo principal
        while (_markerQueue.TryDequeue(out string marker))
            ProcesarMarcador(marker);

        // Pausa con Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            AlternarPausa();

    }

#if UNITY_EDITOR
    [ContextMenu("TEST — Ritmo (Alberto)")]
    private void TestRitmo() => ActivarEventoPersonaje(alberto, rhythmMinigame, bpmPorDefecto);

    [ContextMenu("TEST — Afinacion (Aura)")]
    private void TestAfinacion() => ActivarEventoPersonaje(aura, tuningMinigame, bpmPorDefecto);

    [ContextMenu("TEST — Respiracion (Ruby)")]
    private void TestRespiracion() => ActivarEventoPersonaje(ruby, breathingMinigame, bpmPorDefecto);
#endif

    private void OnDestroy()
    {
        if (_fmodInstance.isValid())
        {
            _fmodInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _fmodInstance.release();
        }
    }

    // ================================================================== //
    //  Inicio de partida
    // ================================================================== //

    private void IniciarPartida()
    {
        _puntuacion      = 0;
        _juegoActivo     = true;
        _pausado         = false;
        _partidaTerminada = false;

        ActualizarScoreUI();

        Debug.Log($"[MM] IniciarPartida — aura:{aura != null} alberto:{alberto != null} ruby:{ruby != null} ramon:{ramon != null}");
        Debug.Log($"[MM] Minijuegos — rhythm:{rhythmMinigame != null} tuning:{tuningMinigame != null} breathing:{breathingMinigame != null}");
        Debug.Log($"[MM] EventPath: '{eventPath}'  instancia válida: {_fmodInstance.isValid()}");

        aura?.IniciarJuego();
        alberto?.IniciarJuego();
        ruby?.IniciarJuego();
        ramon?.IniciarJuego();

        _fmodInstance.start();
        Debug.Log("[MM] FMOD event.start() llamado");
    }

    // ================================================================== //
    //  Callback FMOD (hilo de audio — solo ConcurrentQueue, nunca Unity API)
    // ================================================================== //

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    private static FMOD.RESULT TimelineCallback(
        FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr paramPtr)
    {
        if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
        {
            var props = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)
                Marshal.PtrToStructure(paramPtr, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
            _markerQueue.Enqueue(props.name);
        }
        else if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        {
            var props = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)
                Marshal.PtrToStructure(paramPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
            _beatQueue.Enqueue(props.tempo);
        }
        return FMOD.RESULT.OK;
    }

    // ================================================================== //
    //  Procesado de marcadores
    // ================================================================== //

    private void ProcesarMarcador(string nombre)
    {
        Debug.Log($"[MM] Marcador recibido: '{nombre}'");

        switch (nombre)
        {
            // --- Eventos individuales ---
            case "event_aura_verse1":
                ActivarEventoPersonaje(aura, tuningMinigame, bpmPorDefecto);
                break;

            case "event_ruby_verse1":
                ActivarEventoPersonaje(ruby, breathingMinigame, bpmPorDefecto);
                break;

            case "event_alberto_chorus":
                ActivarEventoPersonaje(alberto, rhythmMinigame, bpmPorDefecto);
                break;

            // --- Eventos simultáneos ---
            case "event_double_verse2":
                ActivarEventoPersonaje(aura,  tuningMinigame,   bpmPorDefecto);
                ActivarEventoPersonaje(ruby,  breathingMinigame, bpmPorDefecto);
                break;

            case "event_climax_bridge":
                ActivarEventoPersonaje(aura,    tuningMinigame,   bpmPorDefecto);
                ActivarEventoPersonaje(alberto, rhythmMinigame,   bpmPorDefecto);
                ActivarEventoPersonaje(ruby,    breathingMinigame, bpmPorDefecto);
                // Ramon: solo drain, sin ventana de minijuego (sin tipo asignado)
                ramon?.ActivarEvento();
                break;

            // --- Eventos especiales ---
            case "event_solo_resolve":
                ManejarSoloResolve();
                break;

            case "event_outro":
                ManejarOutro();
                break;
        }
    }

    // ------------------------------------------------------------------ //
    //  Activación de evento + minijuego para un personaje
    // ------------------------------------------------------------------ //

    private void ActivarEventoPersonaje<T>(CharacterData personaje, T minijuego, float bpm)
        where T : MonoBehaviour
    {
        Debug.Log($"[MM] ActivarEventoPersonaje — personaje:{personaje?.characterName ?? "NULL"} minijuego:{minijuego?.GetType().Name ?? "NULL"}");

        if (personaje == null) { Debug.LogWarning("[MM] personaje es NULL, abortando"); return; }
        if (minijuego == null) { Debug.LogWarning("[MM] minijuego es NULL, abortando"); return; }

        personaje.ActivarEvento();
        Debug.Log($"[MM] ActivarEvento() llamado en {personaje.characterName}");

        // Llamar Activate según el tipo concreto
        if      (minijuego is RhythmMinigame    r) { Debug.Log("[MM] → RhythmMinigame.Activate()");    float actualBpm = _currentBpm > 0f ? _currentBpm : bpm; float beatStart = _lastBeatTime > 0f ? _lastBeatTime : Time.time; r.Activate(personaje, actualBpm, beatStart, OnMinijuegoCompletado); }
        else if (minijuego is TuningMinigame    t) { Debug.Log("[MM] → TuningMinigame.Activate()");   t.Activate(personaje, bpm, OnMinijuegoCompletado); }
        else if (minijuego is BreathingMinigame b) { Debug.Log("[MM] → BreathingMinigame.Activate()"); b.Activate(personaje, bpm, OnMinijuegoCompletado); }
        else { Debug.LogWarning($"[MM] Tipo de minijuego no reconocido: {minijuego.GetType().Name}"); }
    }

    // ------------------------------------------------------------------ //
    //  Callback de minijuego completado
    // ------------------------------------------------------------------ //

    private void OnMinijuegoCompletado(CharacterData personaje, MinigameResult resultado)
    {
        personaje.DesactivarEvento();

        switch (resultado)
        {
            case MinigameResult.Perfect:
                personaje.AñadirEstabilidad(45f);
                AñadirPuntos(100);
                break;
            case MinigameResult.Acceptable:
                personaje.AñadirEstabilidad(25f);
                AñadirPuntos(50);
                break;
            case MinigameResult.Failed:
                // Sin recuperación, sin puntos
                break;
        }
    }

    // ================================================================== //
    //  Pausa
    // ================================================================== //

    public void AlternarPausa()
    {
        if (_pausado) StartCoroutine(ReanudarConCuentaAtras());
        else          PausarJuego();
    }

    private void PausarJuego()
    {
        _pausado = true;
        _fmodInstance.setPaused(true);
        SetPausadoTodos(true);
        rhythmMinigame?.SetPausado(true);
        tuningMinigame?.SetPausado(true);
        breathingMinigame?.SetPausado(true);
        pausaCanvas?.SetActive(true);
    }

    private IEnumerator ReanudarConCuentaAtras()
    {
        pausaCanvas?.SetActive(false);

        if (cuentaAtrasText != null)
        {
            cuentaAtrasText.gameObject.SetActive(true);
            for (int i = 3; i >= 1; i--)
            {
                cuentaAtrasText.text = i.ToString();
                yield return new WaitForSecondsRealtime(1f);
            }
            cuentaAtrasText.gameObject.SetActive(false);
        }

        _pausado = false;
        SetPausadoTodos(false);
        rhythmMinigame?.SetPausado(false);
        tuningMinigame?.SetPausado(false);
        breathingMinigame?.SetPausado(false);
        _fmodInstance.setPaused(false);
    }

    private void SetPausadoTodos(bool pausado)
    {
        aura?.SetPausado(pausado);
        alberto?.SetPausado(pausado);
        ruby?.SetPausado(pausado);
        ramon?.SetPausado(pausado);
    }

    // ================================================================== //
    //  Puntuación
    // ================================================================== //

    private void AñadirPuntos(int cantidad)
    {
        _puntuacion = Mathf.Max(0, _puntuacion + cantidad);
        ActualizarScoreUI();
    }

    public void OnPersonajeCritico()
    {
        _puntuacion = Mathf.Max(0, _puntuacion - 30);
        ActualizarScoreUI();
    }

    private void ActualizarScoreUI()
    {
        if (scoreText != null) scoreText.text = $"Puntuación: {_puntuacion}";
    }

    // ================================================================== //
    //  Game Over — personaje llega a 0
    // ================================================================== //

    public void OnPersonajeFallado(CharacterData personaje)
    {
        if (_partidaTerminada) return;
        _personajeFallado = personaje;
        if (_gameOverTimer != null) StopCoroutine(_gameOverTimer);
        _gameOverTimer = StartCoroutine(TimerGameOver());
    }

    public void OnEstabilidadRecuperada(CharacterData personaje)
    {
        if (_personajeFallado == personaje && _gameOverTimer != null)
        {
            StopCoroutine(_gameOverTimer);
            _gameOverTimer    = null;
            _personajeFallado = null;
        }
    }

    private IEnumerator TimerGameOver()
    {
        yield return new WaitForSeconds(3f);
        if (_personajeFallado != null && _personajeFallado.Stability <= 0f)
            ActivarGameOver();
    }

    private void ActivarGameOver()
    {
        if (_partidaTerminada) return;
        TerminarPartida();
        gameOverCanvas?.SetActive(true);
    }

    // ================================================================== //
    //  Marcadores especiales
    // ================================================================== //

    private void ManejarSoloResolve()
    {
        if (alberto == null) return;
        float estabilidadAlberto = alberto.Stability;
        Debug.Log($"[Solo Resolve] Estabilidad de Alberto: {estabilidadAlberto:F1}");
        // Aquí puedes usar estabilidadAlberto para modificar parámetros de FMOD
        // o registrar el estado del solo para la narrativa final.
    }

    private void ManejarOutro()
    {
        if (_partidaTerminada) return;
        TerminarPartida();
        MostrarResultados();
    }

    // ================================================================== //
    //  Fin de partida (común a Game Over y Outro)
    // ================================================================== //

    private void TerminarPartida()
    {
        _partidaTerminada = true;
        _juegoActivo      = false;
        _fmodInstance.setPaused(true);
        SetPausadoTodos(true);
        rhythmMinigame?.SetPausado(true);
        tuningMinigame?.SetPausado(true);
        breathingMinigame?.SetPausado(true);
    }

    private void MostrarResultados()
    {
        if (conclusionText    != null) conclusionText.text     = ConstruirConclusion();
        if (puntuacionFinalText != null) puntuacionFinalText.text = $"Puntuación final: {_puntuacion}";
        resultadosCanvas?.SetActive(true);
    }

    private string ConstruirConclusion()
    {
        int enPositivo = 0;
        CharacterData[] todos = { aura, alberto, ruby, ramon };
        foreach (var c in todos)
        {
            if (c != null && c.Estado == CharacterData.EstadoEstabilidad.Positivo)
                enPositivo++;
        }

        return enPositivo switch
        {
            4 => "¡Concierto perfecto! El grupo terminó en plena forma.",
            3 => "Gran actuación. Casi todo el grupo dio lo mejor de sí.",
            2 => "Actuación tensa. El grupo sobrevivió, pero con cicatrices.",
            1 => "Concierto al límite. Apenas quedó algo en pie.",
            _ => "El caos se apoderó del escenario. Noche para olvidar."
        };
    }

    // ================================================================== //
    //  Botón de reanudar (desde el Canvas de pausa, asignar en Inspector)
    // ================================================================== //
    public void BotonReanudar()
    {
        if (_pausado) StartCoroutine(ReanudarConCuentaAtras());
    }
}
