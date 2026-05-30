using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public EventReference eventPath;

    // ------------------------------------------------------------------ //
    //  Minijuegos
    // ------------------------------------------------------------------ //
    [Header("Minijuegos")]
    public RhythmMinigame    rhythmMinigame;
    public TuningMinigame    tuningMinigame;
    public BreathingMinigame breathingMinigame;

    // ------------------------------------------------------------------ //
    //  BPM
    // ------------------------------------------------------------------ //
    [Header("BPM de la canción")]
    public float bpmPorDefecto = 160f;

    // ------------------------------------------------------------------ //
    //  UI — Puntuación
    // ------------------------------------------------------------------ //
    [Header("UI — Puntuación")]
    public TextMeshProUGUI scoreText;

    // ------------------------------------------------------------------ //
    //  UI — Fin de partida
    // ------------------------------------------------------------------ //
    [Header("UI — Fin de partida")]
    public GameObject      gameOverCanvas;
    public GameObject      resultadosCanvas;
    public TextMeshProUGUI conclusionText;
    public TextMeshProUGUI puntuacionFinalText;
    public PauseMenu       pauseMenu;

    // ------------------------------------------------------------------ //
    //  Estado interno
    // ------------------------------------------------------------------ //
    private int  _puntuacion;
    private bool _juegoActivo;
    private bool _pausado;
    private bool _partidaTerminada;

    private Coroutine   _gameOverTimer;
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

    // ------------------------------------------------------------------ //
    //  Sistema de burbujas
    // ------------------------------------------------------------------ //
    private readonly Dictionary<CharacterData, int> _pendingCounts = new Dictionary<CharacterData, int>();
    private CharacterData _minijuegoPersonajeActivo;

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
        gameOverCanvas?.SetActive(false);
        resultadosCanvas?.SetActive(false);

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

        while (_beatQueue.TryDequeue(out float tempo))
        {
            _lastBeatTime = Time.time;
            if (tempo > 0f) _currentBpm = tempo;
            OnBeat?.Invoke(_currentBpm);
        }

        while (_markerQueue.TryDequeue(out string marker))
            ProcesarMarcador(marker);

    }

#if UNITY_EDITOR
    [ContextMenu("TEST — Ritmo (Alberto)")]
    private void TestRitmo()
    {
        alberto?.ActivarEvento();
        alberto?.AbrirMinijuego();
        AbrirMinijuegoPorTipo(alberto);
    }

    [ContextMenu("TEST — Afinacion (Aura)")]
    private void TestAfinacion()
    {
        aura?.ActivarEvento();
        aura?.AbrirMinijuego();
        AbrirMinijuegoPorTipo(aura);
    }

    [ContextMenu("TEST — Respiracion (Ruby)")]
    private void TestRespiracion()
    {
        ruby?.ActivarEvento();
        ruby?.AbrirMinijuego();
        AbrirMinijuegoPorTipo(ruby);
    }
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
        _puntuacion               = 0;
        _juegoActivo              = true;
        _pausado                  = false;
        _partidaTerminada         = false;
        _minijuegoPersonajeActivo = null;
        _pendingCounts.Clear();

        ActualizarScoreUI();

        aura?.IniciarJuego();
        alberto?.IniciarJuego();
        ruby?.IniciarJuego();
        ramon?.IniciarJuego();

        _fmodInstance.start();
    }

    // ================================================================== //
    //  Callback FMOD (hilo de audio)
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
        switch (nombre)
        {
            case "event_alberto_intro":
                ActivarEventoPersonaje(alberto);
                break;

            case "event_aura_verse1":
                ActivarEventoPersonaje(aura);
                break;

            case "event_ruby_verse1":
                ActivarEventoPersonaje(ruby);
                break;

            case "event_alberto_chorus":
                ActivarEventoPersonaje(alberto);
                break;

            case "event_double_verse2":
                ActivarEventoPersonaje(aura);
                ActivarEventoPersonaje(ruby);
                break;

            case "event_climax_bridge":
                ActivarEventoPersonaje(aura);
                ActivarEventoPersonaje(alberto);
                ActivarEventoPersonaje(ruby);
                ActivarEventoPersonaje(ramon);
                break;

            case "event_solo_resolve":
                ManejarSoloResolve();
                break;

            case "event_outro":
                ManejarOutro();
                break;
        }
    }

    // ------------------------------------------------------------------ //
    //  Activar evento: muestra burbuja y empieza drain
    // ------------------------------------------------------------------ //

    private void ActivarEventoPersonaje(CharacterData personaje)
    {
        if (personaje == null) return;
        personaje.ActivarEvento();

        if (personaje.tipoMinijuego == TipoMinijuego.Ninguno) return;

        AddPending(personaje);
        personaje.MostrarBurbuja();

        if (_minijuegoPersonajeActivo != null)
            personaje.SetBurbujaInteractuable(false);
    }

    // ------------------------------------------------------------------ //
    //  El jugador clica la burbuja → abre el minijuego
    // ------------------------------------------------------------------ //

    public void OnBubbleClicked(CharacterData personaje)
    {
        if (personaje == null || _minijuegoPersonajeActivo != null || _partidaTerminada) return;

        _minijuegoPersonajeActivo = personaje;
        personaje.OcultarBurbuja();
        personaje.AbrirMinijuego();

        CharacterData[] todos = { aura, alberto, ruby, ramon };
        foreach (var c in todos)
            if (c != null && c != personaje)
                c.SetBurbujaInteractuable(false);

        AbrirMinijuegoPorTipo(personaje);
    }

    private void AbrirMinijuegoPorTipo(CharacterData personaje)
    {
        if (personaje == null) return;
        float actualBpm = _currentBpm > 0f ? _currentBpm : bpmPorDefecto;
        float beatStart = _lastBeatTime > 0f ? _lastBeatTime : Time.time;

        switch (personaje.tipoMinijuego)
        {
            case TipoMinijuego.Ritmo:
                rhythmMinigame?.Activate(personaje, actualBpm, beatStart, OnMinijuegoCompletado);
                break;
            case TipoMinijuego.Afinacion:
                tuningMinigame?.Activate(personaje, actualBpm, OnMinijuegoCompletado);
                break;
            case TipoMinijuego.Respiracion:
                breathingMinigame?.Activate(personaje, actualBpm, OnMinijuegoCompletado);
                break;
        }
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
        }

        SubPending(personaje);
        _minijuegoPersonajeActivo = null;

        if (GetPending(personaje) > 0)
            personaje.MostrarBurbuja();

        CharacterData[] todos = { aura, alberto, ruby, ramon };
        foreach (var c in todos)
            if (c != null && GetPending(c) > 0)
                c.SetBurbujaInteractuable(true);
    }

    // ------------------------------------------------------------------ //
    //  Helpers pendientes
    // ------------------------------------------------------------------ //

    private int  GetPending(CharacterData c) => _pendingCounts.TryGetValue(c, out int v) ? v : 0;
    private void AddPending(CharacterData c) => _pendingCounts[c] = GetPending(c) + 1;
    private void SubPending(CharacterData c) => _pendingCounts[c] = Mathf.Max(0, GetPending(c) - 1);

    // ================================================================== //
    //  Pausa — API para PauseMenu
    // ================================================================== //

    public bool IsPartidaTerminada => _partidaTerminada;

    public void PausarDesdeMenu()
    {
        if (_pausado || _partidaTerminada) return;
        _pausado = true;
        _fmodInstance.setPaused(true);
        SetPausadoTodos(true);
        rhythmMinigame?.SetPausado(true);
        tuningMinigame?.SetPausado(true);
        breathingMinigame?.SetPausado(true);
    }

    public void ReanudarDesdeMenu()
    {
        if (!_pausado) return;
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
        if (scoreText != null) scoreText.text = $"{_puntuacion}";
    }

    // ================================================================== //
    //  Game Over
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
        pauseMenu?.MostrarMenuFinal();
    }

    // ================================================================== //
    //  Marcadores especiales
    // ================================================================== //

    private void ManejarSoloResolve()
    {
        if (alberto == null) return;
    }

    private void ManejarOutro()
    {
        if (_partidaTerminada) return;
        TerminarPartida();
        MostrarResultados();
    }

    // ================================================================== //
    //  Fin de partida
    // ================================================================== //

    private void TerminarPartida()
    {
        _partidaTerminada = true;
        _juegoActivo      = false;

        if (_minijuegoPersonajeActivo != null)
        {
            _minijuegoPersonajeActivo.DesactivarEvento();
            _minijuegoPersonajeActivo = null;
        }

        aura?.DesactivarEvento();
        alberto?.DesactivarEvento();
        ruby?.DesactivarEvento();
        ramon?.DesactivarEvento();

        _fmodInstance.setPaused(true);
        SetPausadoTodos(true);
        rhythmMinigame?.SetPausado(true);
        tuningMinigame?.SetPausado(true);
        breathingMinigame?.SetPausado(true);
    }

    private void MostrarResultados()
    {
        if (conclusionText      != null) conclusionText.text     = ConstruirConclusion();
        if (puntuacionFinalText != null) puntuacionFinalText.text = $"Puntuación final: {_puntuacion}";
        resultadosCanvas?.SetActive(true);
        pauseMenu?.MostrarMenuFinal();
    }

    private string ConstruirConclusion()
    {
        int enPositivo = 0;
        CharacterData[] todos = { aura, alberto, ruby, ramon };
        foreach (var c in todos)
            if (c != null && c.Estado == CharacterData.EstadoEstabilidad.Positivo)
                enPositivo++;

        return enPositivo switch
        {
            4 => "¡Concierto perfecto! El grupo terminó en plena forma.",
            3 => "Gran actuación. Casi todo el grupo dio lo mejor de sí.",
            2 => "Actuación tensa. El grupo sobrevivió, pero con cicatrices.",
            1 => "Concierto al límite. Apenas quedó algo en pie.",
            _ => "El caos se apoderó del escenario. Noche para olvidar."
        };
    }

}
