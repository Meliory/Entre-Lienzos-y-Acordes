# 4. Videojuego: Demo

## 4.1 Introducción y alcance de la demo

### Descripción general

La demo de "Entre Lienzos y Acordes" constituye la tercera fase del Trabajo de Final de Grado. Su objetivo no es presentar un videojuego completo, sino demostrar de forma funcional cómo la banda sonora original compuesta en la segunda fase del proyecto se integra en distintos contextos de juego: cinemáticas, escenarios de exploración con música dinámica y minijuegos musicales.

La demo se ha diseñado con un alcance deliberadamente acotado. Se ha priorizado la calidad técnica de los sistemas de integración musical sobre la cantidad de contenido jugable. El resultado es un conjunto de escenas funcionales que ilustran tres modalidades distintas de uso de la música en tiempo real.

### Contenido incluido en la demo

La demo incluye cuatro escenas jugables:

- **Menú principal:** punto de entrada de la aplicación con acceso al resto de escenas.
- **Cinemática:** secuencia audiovisual en la que se reproduce una pieza de la banda sonora sincronizada con movimientos de cámara y subtítulos de diálogo, implementada mediante Unity Timeline y Cinemachine.
- **Escenario de exploración ("LoopSongs"):** escena en la que el jugador se desplaza por un entorno tridimensional y la música se activa, mezcla y transforma dinámicamente en función de su posición mediante zonas de colisión.
- **Minijuego musical:** escena en la que cuatro personajes (Aura, Alberto, Ruby y Ramón) presentan eventos sincronizados con los marcadores de la banda sonora, que el jugador debe resolver mediante tres mecánicas distintas de interacción.

### Contenido deliberadamente excluido

Quedan fuera del alcance de la demo los siguientes elementos:

- El arco narrativo completo de la novela. Los personajes y su contexto se presentan de forma mínima, suficiente para que la demo sea comprensible, pero sin desarrollar la trama.
- La implementación en juego de las 17 piezas de la banda sonora. La demo contempla un subconjunto representativo.
- Un sistema de guardado de partida o progresión entre sesiones.
- Optimización de rendimiento para hardware de gama baja.
- Soporte para plataformas distintas de PC con Windows.

### Justificación de la demo frente a un videojuego completo

El desarrollo de un videojuego completo excede el alcance temporal y los recursos individuales de un TFG. La decisión de producir una demo responde a dos criterios: (1) demostrar que los sistemas técnicos diseñados son funcionales y extensibles, y (2) garantizar que el vínculo entre banda sonora y diseño de juego queda documentado y verificable por el tribunal.

### Conexión con la novela y la banda sonora

Los cuatro personajes del minijuego (Aura, Alberto, Ruby y Ramón) son protagonistas de la novela autopublicada que constituye la primera fase del TFG. Cada personaje tiene asociada una o varias piezas de la banda sonora que reflejan su estado emocional en momentos clave de la historia. En la demo, esta asociación se materializa de dos formas: a nivel diegético, mediante la música dinámica del escenario de exploración, y a nivel mecánico, mediante la barra de estabilidad de cada personaje en el minijuego, que se degrada si el jugador no atiende los eventos musicales a tiempo.

---

## 4.2 Decisiones técnicas y herramientas

### Motor de videojuego

Se ha utilizado **Unity 6** (versión **6000.3.8f1**) como motor de desarrollo. La elección se justifica por los siguientes motivos:

- Soporte nativo para el pipeline Universal Render Pipeline (URP), que ofrece un equilibrio adecuado entre calidad visual y rendimiento en PC.
- Integración oficial con FMOD Studio mediante el paquete "FMOD for Unity", que es el middleware de audio seleccionado para el proyecto.
- Disponibilidad de herramientas de secuenciación temporal (Unity Timeline) y de cámara cinematográfica (Cinemachine) como paquetes de primera parte, sin necesidad de soluciones de terceros para las cinemáticas.
- Familiaridad previa del autor con el entorno de desarrollo.

### Lenguaje de programación

Todos los scripts del proyecto están escritos en **C#**, que es el único lenguaje de scripting soportado por Unity en su versión actual. Se han empleado las siguientes características del lenguaje relevantes para la arquitectura del proyecto:

- Genéricos y colecciones tipadas (`Dictionary<K,V>`, `List<T>`).
- Delegados y eventos (`Action<T>`, `event`).
- Colecciones seguras para hilos (`ConcurrentQueue<T>`), necesarias para el sistema de callbacks de FMOD.
- Atributos personalizados (`[Header]`, `[Range]`, `[SerializeField]`, `[TextArea]`) para la configuración desde el Inspector de Unity.
- Corrutinas (`IEnumerator`, `StartCoroutine`) para la gestión de secuencias temporales sin bloqueo del hilo principal.

### Middleware de audio: FMOD Studio

Se ha integrado **FMOD Studio** (versión **2.02.34**) mediante el paquete oficial **FMOD for Unity** (versión **2.02.34**). FMOD actúa como motor de audio en tiempo de ejecución y como herramienta de autoría de los bancos de audio.

Las razones de la elección de FMOD frente a otras alternativas (audio nativo de Unity, Wwise) son:

- Soporte para marcadores temporales en el timeline del evento (`TIMELINE_MARKER`), que permiten disparar eventos de juego sincronizados con puntos concretos de la música.
- Soporte para el callback `TIMELINE_BEAT`, que expone el pulso de la pieza musical en tiempo real con el BPM exacto, necesario para la sincronización del minijuego de ritmo.
- Sistema de parámetros globales que permite modificar el comportamiento del audio (mezcla de pistas, efectos de distorsión) desde el código del juego en tiempo real.
- Posibilidad de separar las pistas de instrumento dentro de un mismo evento y controlarlas individualmente mediante parámetros.

### Otras herramientas

- **Control de versiones:** Git con repositorio remoto en GitHub. Rama principal: `main`.
- **Pipeline de renderizado:** Universal Render Pipeline (URP) de Unity, configurado con los assets `PC_RPAsset` y `PC_Renderer`.
- **Cámara cinematográfica:** paquete Cinemachine de Unity, utilizado en la escena de cinemática.
- **Secuenciación temporal:** paquete Unity Timeline, utilizado para la cinemática y la sincronización de señales de diálogo.
- **Tipografía UI:** TextMesh Pro (TMP), integrado en Unity 6.

### Resolución y plataforma objetivo

- **Plataforma objetivo:** PC con Windows.
- **Resolución objetivo:** — pendiente de confirmar por el autor —.
- **Formato de distribución:** build de escritorio generado desde el editor de Unity.

---

## 4.3 Arquitectura general del proyecto

### Estructura de carpetas

El proyecto sigue la estructura de carpetas estándar de Unity, con las siguientes subcarpetas relevantes bajo `Assets/`:

```
Assets/
├── Audio/                        # Archivos de audio (ej. Ápice de Estrellas.wav)
├── Plugins/FMOD/                 # Paquete FMOD for Unity 2.02.34
├── Scenes/                       # Escenas Unity (.unity): MainMenu, Cinematica,
│                                 # LoopSongs, Minijuego
├── Scripts/
│   ├── Cinematica/               # CinematicaManager.cs, CinematicaDialogo.cs
│   ├── LoopSongs/                # MusicManager.cs, ZonaCancion.cs, ZonaPista.cs,
│   │                             # MusicUI.cs, ZoneShaderManager.cs,
│   │                             # PlayerController.cs, CameraController.cs
│   └── MusicMinigame/            # MinigameManager.cs, CharacterData.cs,
│                                 # RhythmMinigame.cs, TuningMinigame.cs,
│                                 # BreathingMinigame.cs
├── Settings/                     # PC_RPAsset.asset, PC_Renderer.asset (URP)
└── — pendiente de confirmar por el autor — (prefabs, materiales, texturas, etc.)
```

### Patrones de diseño aplicados

**Singleton.** Los gestores globales `MinigameManager`, `MusicManager`, `MusicUI` y `ZoneShaderManager` implementan el patrón Singleton mediante una propiedad estática `Instance` asignada en `Awake()`. Esto garantiza que exista una única instancia activa por escena y que cualquier componente pueda acceder a ella sin referencias directas en el Inspector.

```csharp
// Assets/Scripts/MusicMinigame/MinigameManager.cs
private void Awake()
{
    if (Instance != null && Instance != this) { Destroy(gameObject); return; }
    Instance = this;
}
```

**Observador (evento estático).** El minijuego de ritmo requiere que múltiples componentes reaccionen al pulso musical en tiempo real. Se ha implementado un evento estático en `MinigameManager` al que `RhythmMinigame` se suscribe mientras está activo:

```csharp
// Assets/Scripts/MusicMinigame/MinigameManager.cs
public static event Action<float> OnBeat;
```

```csharp
// Assets/Scripts/MusicMinigame/RhythmMinigame.cs
MinigameManager.OnBeat += OnBeatRecibido;   // al activarse
MinigameManager.OnBeat -= OnBeatRecibido;   // al desactivarse
```

**Productor-consumidor con cola concurrente.** El callback de FMOD se ejecuta en el hilo de audio, no en el hilo principal de Unity. Para evitar condiciones de carrera, los datos generados por el callback (marcadores y pulsos) se depositan en colas `ConcurrentQueue<T>` y se consumen en el `Update()` del hilo principal.

**Callback con delegado.** Cada minijuego recibe al activarse un delegado `Action<CharacterData, MinigameResult>` que invoca al completarse. Esto desacopla el minijuego del gestor: el minijuego no necesita saber quién lo ha lanzado, solo a quién notificar cuando termina.

### Sistema de escenas

La demo está organizada en cuatro escenas de Unity, alojadas en `Assets/Scenes/`:

- **MainMenu:** punto de entrada de la aplicación.
- **Cinematica:** secuencia audiovisual de introducción.
- **LoopSongs:** escenario de exploración con música dinámica.
- **Minijuego:** escena del minijuego musical con los cuatro personajes.

### Sistema de gestión del audio

El audio se gestiona íntegramente a través de FMOD Studio. Desde Unity, el audio se controla mediante instancias de `FMOD.Studio.EventInstance` creadas en tiempo de ejecución. No se utiliza el sistema de audio nativo de Unity (AudioSource/AudioClip) para la música principal.

Los parámetros globales de FMOD (tipo `float`, rango 0-100 para estabilidad y 0-1 para distorsión) se actualizan cada fotograma desde `CharacterData` mediante `RuntimeManager.StudioSystem.setParameterByName()`.

---

## 4.4 Implementación de las cinemáticas

### Solución técnica

La cinemática se ha implementado combinando tres sistemas de Unity:

1. **Unity Timeline:** gestiona la secuencia completa de la escena (movimientos de cámara, activación de objetos, disparos de señal para diálogo y efectos de prop).
2. **Cinemachine:** proporciona las cámaras virtuales que el Timeline controla mediante un `CinemachineTrack`. Los movimientos de cámara se definen como keyframes de posición y rotación interpolados en el Timeline.
3. **FMOD (CinematicaManager.cs):** inicia la reproducción del evento de audio al arrancar la escena.

La sincronización entre la música y la acción visual se consigue posicionando manualmente los keyframes, objetos y señales de diálogo en el Timeline en los instantes que corresponden con los puntos musicales deseados. Este enfoque garantiza que la sincronización sea determinista e independiente de la latencia del sistema de audio.

### Script de arranque de audio: CinematicaManager.cs

```csharp
// Assets/Scripts/Cinematica/CinematicaManager.cs
using FMODUnity;
using UnityEngine;

public class CinematicaManager : MonoBehaviour
{
    [Header("FMOD")]
    public EventReference musicaEvento;

    private FMOD.Studio.EventInstance _musicaInstance;

    private void Start()
    {
        _musicaInstance = RuntimeManager.CreateInstance(musicaEvento);
        _musicaInstance.start();
    }

    private void OnDestroy()
    {
        if (_musicaInstance.isValid())
        {
            _musicaInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _musicaInstance.release();
        }
    }
}
```

El script crea una instancia del evento FMOD al inicio de la escena y la libera con fadeout al destruirse. Se utiliza `STOP_MODE.ALLOWFADEOUT` para que FMOD aplique la cola de fade definida en el propio evento, sin corte abrupto.

### Sistema de diálogo por señales: CinematicaDialogo.cs

Los subtítulos de diálogo se disparan mediante un **Signal Track** en el Timeline. Cada señal invoca el método `MostrarSiguiente()` de `CinematicaDialogo`, que avanza al siguiente elemento del array `LineaDialogo[]` y muestra el panel del personaje correspondiente, ocultando los demás:

```csharp
// Assets/Scripts/Cinematica/CinematicaDialogo.cs
public void MostrarSiguiente()
{
    _lineaActual++;
    if (_lineaActual >= lineas.Length) return;

    var linea = lineas[_lineaActual];

    foreach (var p in paneles)
    {
        bool esSuyo = p.personaje == linea.personaje;
        p.panel?.SetActive(esSuyo);
        if (esSuyo)
        {
            if (p.speakerText)  p.speakerText.text  = linea.personaje;
            if (p.dialogueText) p.dialogueText.text = linea.texto;
        }
    }
}
```

El script expone dos arrays configurables en el Inspector: `LineaDialogo[]`, que contiene el texto de cada línea y el nombre del personaje que la pronuncia, y `PanelPersonaje[]`, que asocia cada nombre de personaje con su panel de UI, su campo de locutor y su campo de diálogo. La señal `OcultarDialogo` invoca el método `Ocultar()`, que desactiva todos los paneles.

Adicionalmente, el script gestiona dos props de la escena mediante señales específicas: `ApagarVela()`, que extingue un componente `Fire` (vela encendida), y `EsconderVela()`, que desactiva el GameObject de la vela. Ambas acciones se sincronizan con puntos concretos de la música mediante señales adicionales en el Timeline.

[CAPTURA: Vista del Timeline de la cinemática con las pistas de Cinemachine, la Signal Track con las señales de diálogo y de props, y la forma de onda de la pieza visible en el editor de Unity]

### Caso concreto: "Ápice de Estrellas"

La pieza "Ápice de Estrellas" se utiliza como banda sonora de la cinemática. Los cambios de cámara, las apariciones de diálogo y los efectos de prop (encendido y apagado de vela) se han posicionado en el Timeline para coincidir con los momentos estructurales de la pieza (— pendiente de confirmar por el autor —: descripción de los puntos de sincronización concretos: compases o segundos exactos donde se producen los cambios principales).

---

## 4.5 Implementación de los escenarios (música de fondo)

### Arquitectura del sistema de música dinámica

El sistema de música en el escenario de exploración (escena "LoopSongs") se basa en cuatro clases que actúan de forma coordinada:

- **`MusicManager`** (Singleton): gestiona la instancia de evento FMOD activa, la lógica de crossfade entre pistas y el inicio y detención del audio.
- **`ZonaCancion`**: componente con un `Collider` en modo trigger que, cuando el jugador entra, ordena a `MusicManager` que inicie el evento FMOD correspondiente. Al salir, ordena detenerlo con un fadeout.
- **`ZonaPista`**: zonas secundarias dentro de una `ZonaCancion` que controlan qué pista del evento FMOD está activa mediante un parámetro de nombre configurable en el Inspector (`paramName`). Cada `ZonaPista` tiene además un `displayName` para la UI y un `tintColor` para el feedback visual de postprocesado.
- **`ZoneShaderManager`** (Singleton): gestiona el tinte de color de la escena mediante el efecto `ColorAdjustments` de URP Post-Processing. Cuando el jugador entra en una `ZonaPista`, el tinte de la escena transita suavemente al color asignado a esa pista.

### Crossfade dinámico

El crossfade entre pistas se gestiona íntegramente en `MusicManager.Update()`. El sistema mantiene un `Dictionary<ZonaPista, float>` con el valor actual del parámetro de cada pista. Cada fotograma, el valor de la pista activa se aproxima a 1 y el resto a 0, con una velocidad determinada por `crossfadeDuration` (valor por defecto: 2 segundos):

```csharp
// Assets/Scripts/LoopSongs/MusicManager.cs
private void Update()
{
    if (!_eventInstance.isValid() || !_eventStarted) return;

    float step = Time.deltaTime / crossfadeDuration;

    foreach (var pista in new List<ZonaPista>(_paramValues.Keys))
    {
        float target = (pista == _currentPista) ? 1f : 0f;
        _paramValues[pista] = Mathf.MoveTowards(_paramValues[pista], target, step);
        _eventInstance.setParameterByName(pista.ParamName, _paramValues[pista]);
    }
}
```

Este valor se envía a FMOD mediante `setParameterByName`. En el evento de FMOD Studio, cada parámetro está conectado al nivel de su pista de instrumento mediante una automatización, de modo que el crossfade en Unity se traduce en un fundido cruzado audible en tiempo real.

### Tinte de postprocesado sincronizado con la zona

Al entrar en una `ZonaPista`, además del crossfade de audio, se activa una transición de color en el post-procesado de URP:

```csharp
// Assets/Scripts/LoopSongs/ZonaPista.cs
private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        MusicManager.Instance.EnterPista(this);
        ZoneShaderManager.Instance.SetTint(tintColor);
    }
}
```

`ZoneShaderManager` interpola el filtro de color (`ColorAdjustments.colorFilter`) del volumen de postprocesado desde el color actual hacia `tintColor` durante `transitionDuration` segundos (valor por defecto: 2 segundos). Al salir de la zona, el tinte vuelve a blanco (`Color.white`).

### Interfaz visual de pistas activas: MusicUI

Cuando el jugador entra en una `ZonaCancion`, `MusicUI` instancia una fila de UI por cada `ZonaPista` hija. Cada fila muestra el `displayName` de la pista y un slider no interactivo cuyo valor refleja en tiempo real el parámetro FMOD correspondiente (0 a 1). Esto proporciona al jugador una representación visual de qué capas musicales están activas en cada momento.

### Zona de canción: activación por colisión

```csharp
// Assets/Scripts/LoopSongs/ZonaCancion.cs
private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        MusicManager.Instance.IniciarEvento(musicEvent);
        MusicUI.Instance.Mostrar(GetComponentsInChildren<ZonaPista>());
    }
}
```

Al entrar en la zona, se inicia el evento FMOD y se muestra la interfaz de pistas activas. Al salir, `DetenerEvento()` ejecuta una corrutina de fadeout que reduce el parámetro de la pista activa a 0 antes de detener y liberar la instancia.

El campo `isStartingByStart` de `ZonaPista` permite configurar que, al entrar en esa pista, el evento se reinicie desde el inicio en lugar de continuar desde la posición actual, lo que resulta útil para secciones que deben empezar siempre desde el principio.

[CAPTURA: Vista de la escena LoopSongs con los colliders de ZonaCancion y ZonaPista visibles, y el panel de MusicUI activo mostrando los sliders de pistas]

### Implementación por canción

Los detalles de configuración específica de cada canción (número de pistas, nombres de parámetros FMOD, colores de tinte asignados a cada ZonaPista) dependen de la configuración en el Inspector de Unity y en FMOD Studio, y se recogen en la siguiente tabla (— pendiente de confirmar por el autor —: completar con los valores exactos de cada pieza implementada en la demo).

---

## 4.6 Implementación de los minijuegos

### Diseño general del sistema

El minijuego musical es la escena de mayor complejidad técnica de la demo. En ella, cuatro personajes (Aura, Alberto, Ruby y Ramón) se presentan en pantalla con una barra de estabilidad que se degrada con el tiempo mientras la música avanza. La música es un evento de FMOD Studio con marcadores temporales que disparan ventanas de minijuego asociadas a personajes concretos.

El sistema se organiza en torno a tres clases principales:

- **`MinigameManager`** (Singleton): orquesta el ciclo de vida completo de la partida. Recibe los callbacks de FMOD, procesa los marcadores, gestiona las burbujas de aviso, abre los minijuegos y recibe su resultado.
- **`CharacterData`**: representa el estado de cada personaje (estabilidad, drain, estado visual, distorsión de audio) y expone una API pública para que `MinigameManager` lo controle.
- **`RhythmMinigame` / `TuningMinigame` / `BreathingMinigame`**: cada una implementa una mecánica de minijuego distinta y notifica su resultado mediante un delegado al completarse.

El enum `MinigameResult` define tres posibles resultados: `Perfect`, `Acceptable` y `Failed`. El enum `TipoMinijuego` (valores: `Ninguno`, `Ritmo`, `Afinacion`, `Respiracion`) está definido en `CharacterData` y configura en el Inspector qué mecánica corresponde a cada personaje.

### Sincronización con la música: callbacks de FMOD

El callback de FMOD se registra en `Start()` para recibir dos tipos de evento: `TIMELINE_MARKER` y `TIMELINE_BEAT`. Dado que este callback se ejecuta en el hilo de audio, los datos se depositan en colas `ConcurrentQueue<T>` y se procesan en `Update()`:

```csharp
// Assets/Scripts/MusicMinigame/MinigameManager.cs
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
```

En `Update()`, los beats procesados actualizan `_lastBeatTime` y `_currentBpm`, y disparan el evento estático `OnBeat` al que `RhythmMinigame` está suscrito. Los marcadores se procesan en un switch que asocia cada nombre de marcador con la acción de juego correspondiente.

Los marcadores definidos en el evento de FMOD Studio son ocho:

| Nombre del marcador      | Efecto en juego                                               |
|--------------------------|---------------------------------------------------------------|
| `event_alberto_intro`    | Activa minijuego de ritmo para Alberto                        |
| `event_aura_verse1`      | Activa minijuego de afinación para Aura                       |
| `event_ruby_verse1`      | Activa minijuego de respiración para Ruby                     |
| `event_alberto_chorus`   | Segunda aparición del minijuego de Alberto                    |
| `event_double_verse2`    | Activa minijuegos de Aura y Ruby simultáneamente              |
| `event_climax_bridge`    | Activa los cuatro personajes simultáneamente                  |
| `event_solo_resolve`     | Marcador especial para Alberto (sin minijuego activo en la demo) |
| `event_outro`            | Finaliza la partida y muestra la pantalla de resultados       |

### Sistema de burbujas: apertura controlada por el jugador

Cuando un marcador activa un evento para un personaje, el sistema no abre el minijuego directamente. En su lugar, incrementa el contador de eventos pendientes del personaje (`_pendingCounts`) y muestra un botón de aviso (la "burbuja") sobre su panel de UI. El drain de estabilidad comienza en este momento.

El jugador decide cuándo abrir el minijuego pulsando la burbuja. En ese instante, `OnBubbleClicked()` verifica que no haya otro minijuego activo, deshabilita la interactividad del resto de burbujas y abre el minijuego correspondiente al `TipoMinijuego` del personaje:

```csharp
// Assets/Scripts/MusicMinigame/MinigameManager.cs
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
```

Al completarse el minijuego, `OnMinijuegoCompletado()` decrementa el contador, re-habilita las burbujas con eventos pendientes y, si el personaje tiene más eventos acumulados, vuelve a mostrar su burbuja.

### Sistema de estabilidad y drain

Cada personaje tiene dos tasas de degradación configurables en el Inspector: `passiveDrain` (drain constante, independiente del estado del evento) y `eventDrain` (drain adicional mientras el evento está activo). Ambos se aplican por segundo en `CharacterData.Update()`:

```csharp
// Assets/Scripts/MusicMinigame/CharacterData.cs
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
```

Los valores de drain configurados en el Inspector son:

| Personaje | passiveDrain | eventDrain |
|-----------|-------------|------------|
| Aura      | 0           | 2.5        |
| Alberto   | 0           | — pendiente de confirmar por el autor — |
| Ruby      | 0.3         | 1.8        |
| Ramón     | 0           | 1.2        |

El estado visual (color de la barra, texto de estado) se actualiza automáticamente en función de umbrales definidos en `CharacterData.UpdateUI()`: Positivo (>= 80), Tenso (>= 50), Desestabilizado (>= 25), Crítico (> 0) y Fallado (= 0).

Cuando la estabilidad cae por primera vez a estado Crítico, `MinigameManager.OnPersonajeCritico()` aplica una penalización de 30 puntos. Si llega a 0 y permanece así durante 3 segundos, se activa el Game Over.

Al completar un minijuego, la estabilidad se restaura según el resultado: +45 para `Perfect`, +25 para `Acceptable`, sin recuperación para `Failed`.

### Distorsión de audio durante el minijuego

Mientras un minijuego está abierto, `CharacterData` empuja un parámetro de distorsión a FMOD proporcional a la estabilidad perdida. La distorsión aumenta a medida que la estabilidad disminuye durante el minijuego, y se restablece a 0 al terminar:

```csharp
// Assets/Scripts/MusicMinigame/CharacterData.cs
private void ActualizarDistorsion()
{
    if (string.IsNullOrEmpty(fmodDistortionParameter)) return;

    float target = MinijuegoAbierto ? (1f - stability / 100f) : 0f;
    float speed  = MinijuegoAbierto ? 1.5f : 2f;
    _distortionActual = Mathf.MoveTowards(_distortionActual, target, speed * Time.deltaTime);
    FMODUnity.RuntimeManager.StudioSystem.setParameterByName(fmodDistortionParameter, _distortionActual);
}
```

En FMOD Studio, los parámetros globales `distortion_aura`, `distortion_alberto` y `distortion_ruby` están conectados a efectos DSP en las pistas de instrumento correspondientes (efecto Delay sobre el Wet Level en el caso de la batería). Al finalizar el minijuego, `DesactivarEvento()` resetea el parámetro a 0 de forma inmediata.

### Minijuego de ritmo (Alberto): RhythmMinigame

El jugador debe pulsar un botón en sincronía con el pulso de la música. El minijuego consta de dos fases:

1. **Fase de observación:** el círculo central pulsa al ritmo de los beats de FMOD (recibidos vía `OnBeat`). Los dots de la UI parpadean en ola. El jugador no está obligado a actuar todavía.
2. **Fase activa:** el primer tap del jugador inicia la fase activa. Se evalúan cuatro taps consecutivos.

La precisión de cada tap se mide contra `_lastFlashBeatTime`, el instante en que el círculo visual pulsó por última vez. Medir contra el flash visual en lugar de contra el beat de FMOD evita que la latencia de audio (imperceptible para el jugador) penalice taps que visualmente son correctos:

```csharp
// Assets/Scripts/MusicMinigame/RhythmMinigame.cs
private void MedirTap()
{
    float distancia = float.MaxValue;
    if (_lastFlashBeatTime >= 0f)
    {
        float despues = Time.time - _lastFlashBeatTime;
        float antes   = (_lastFlashBeatTime + _beatInterval) - Time.time;
        distancia = Mathf.Min(Mathf.Abs(despues), Mathf.Abs(antes));
    }

    float ventPerfecto  = _beatInterval * fraccionPerfecto;   // defecto: 0.25
    float ventAceptable = _beatInterval * fraccionAceptable;  // defecto: 0.40
    // ...
}
```

El resultado es `Perfect` si se obtienen 3 o más taps perfectos, `Acceptable` si se suman 2 o más aciertos totales, y `Failed` en caso contrario.

Las corrutinas de la fase activa esperan el beat usando el flag `_beatFlag` en lugar de `WaitForSeconds`, para mantener la sincronización exacta con el audio independientemente de la carga de CPU:

```csharp
// Assets/Scripts/MusicMinigame/RhythmMinigame.cs
private IEnumerator RutinaTaps()
{
    for (int beat = _tapCount; beat < totalTaps; beat++)
    {
        _beatFlag = false;
        while (!_beatFlag && _activo) yield return null;
        // ...
    }
}
```

### Minijuego de afinación (Aura): TuningMinigame

Una aguja oscila de lado a lado sobre un medidor. El jugador dispone de tres intentos para pulsar el botón "Ajustar" cuando la aguja se encuentra dentro de la zona verde central. La velocidad de la aguja se multiplica tras cada intento (factor `multiplicadorFallo`, valor por defecto: 1.7), incrementando la dificultad progresivamente.

La anchura visual de la zona verde se calcula en tiempo de ejecución mediante geometría trigonométrica: el semiángulo configurado (`semiAnchoZonaVerde`, en grados) se convierte a píxeles en función de la longitud de la aguja, garantizando que el área visual coincida exactamente con la lógica de detección:

```csharp
// Assets/Scripts/MusicMinigame/TuningMinigame.cs
private IEnumerator CalibrateZona()
{
    yield return null; // esperar un frame para que Unity haya calculado el layout
    if (needle == null || zonaVerdeRect == null) yield break;

    float tipDist = needle.rect.height * (1f - needle.pivot.y);
    float halfZone = tipDist * Mathf.Sin(semiAnchoZonaVerde * Mathf.Deg2Rad);
    zonaVerdeRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, halfZone * 2f);
}
```

El resultado es `Perfect` con 3 aciertos, `Acceptable` con 2 y `Failed` con 1 o menos.

### Minijuego de respiración (Ruby): BreathingMinigame

El jugador debe realizar tres ciclos de respiración manteniendo pulsado el botón durante la inhalación y soltándolo durante la exhalación. La duración de cada fase es aleatoria dentro de un rango configurable (`inhalacionMin/Max`, `exhalacionMin/Max`), lo que impide que el jugador anticipe mecánicamente el ritmo.

La evaluación de cada fase mide el tiempo en que el estado del botón es correcto respecto a la duración total. Un ciclo se considera correcto si tanto la inhalación como la exhalación superan el umbral `umbralCorrecto` (valor por defecto: 0.5). Un círculo visual crece y encoge al ritmo de las fases, cambiando de color para indicar si el estado del botón es correcto (verde) o incorrecto (rojo).

[CAPTURA: Los tres paneles de minijuego (ritmo, afinación, respiración) en sus estados activos, con los elementos de UI visibles]

[CAPTURA: Pantalla de resultados con el estado final de los cuatro personajes y la puntuación]

---

## 4.7 Sistema de input y feedback

### Gestión de la entrada del jugador

Se utiliza el **nuevo sistema de Input de Unity** (`UnityEngine.InputSystem`) para la detección del teclado. La tecla Escape activa o desactiva la pausa:

```csharp
// Assets/Scripts/MusicMinigame/MinigameManager.cs
if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
    AlternarPausa();
```

El resto de las interacciones del minijuego utilizan el componente `Button` de Unity UI, cuyo evento `onClick` se suscribe en tiempo de ejecución (`OnEnable`/`OnDisable`) para evitar fugas de memoria al desactivar el panel.

Para el minijuego de respiración, la detección de pulsación mantenida (hold) se implementa mediante `EventTrigger` con eventos `PointerDown` y `PointerUp`, dado que el sistema de botones estándar de Unity no expone el estado de pulsación continua.

### Feedback visual

El feedback visual en el minijuego sigue un esquema consistente en todas las mecánicas:

- **Dots de progreso:** cada minijuego dispone de indicadores circulares (dots) que muestran el resultado de cada intento o ciclo mediante codificación de color (azul claro para pendiente, verde para correcto, rojo para fallo).
- **Texto de feedback:** aparece brevemente tras cada acción del jugador y desaparece automáticamente tras un segundo.
- **Animaciones de escala:** el círculo del minijuego de ritmo pulsa sincronizado con el beat mediante una corrutina que usa interpolación senoidal (`Mathf.Sin`) para un resultado más orgánico que una interpolación lineal.
- **Cambio de color de la aguja:** en `TuningMinigame`, la aguja cambia de blanco a verde en tiempo real cuando se encuentra dentro de la zona válida, proporcionando feedback inmediato sin necesidad de que el jugador pulse.
- **Panel de resultado:** al finalizar cada minijuego se muestra un panel con el resultado textual y la precisión numérica durante 2 segundos antes de cerrar el panel.

### Feedback de pausa

Al pausar, la música se pausa mediante `_fmodInstance.setPaused(true)` y todos los personajes y minijuegos activos quedan suspendidos. Al reanudar, se muestra una cuenta atrás de 3 segundos antes de devolver el control al jugador y reanudar el audio, evitando una transición abrupta.

---

## 4.8 Limitaciones conocidas de la demo

### Funcionalidad no implementada o incompleta

- **Ramón no tiene minijuego asociado.** Su estabilidad se degrada durante `event_climax_bridge` sin posibilidad de recuperación por acción del jugador. Se ha diseñado así de forma deliberada para esta demo, como elemento de presión adicional.
- **`event_solo_resolve` no tiene lógica activa.** El marcador existe en el sistema pero el método `ManejarSoloResolve()` no ejecuta ninguna acción en la versión actual.
- **La imagen de las burbujas** es un asset visual asignado en el Inspector. El código gestiona la visibilidad y la interactividad del botón, pero el sprite concreto es responsabilidad del autor.
- **Los detalles de configuración de las canciones en la escena LoopSongs** (nombres de parámetros FMOD, colores de tinte por zona) están definidos en el Inspector de Unity y en FMOD Studio, y no se derivan del código.

### Limitaciones técnicas

- **Compatibilidad de versiones FMOD:** durante el desarrollo se produjo un cambio de versión del plugin FMOD for Unity (de una versión anterior a la 2.02.34) que implicó renombrar el tipo `EventRef` a `EventReference` en todos los scripts que lo referenciaban, y recompilar los bancos de audio. Los bancos compilados con una versión de FMOD Studio no son compatibles con el runtime de otra versión, lo que requiere mantener ambas versiones sincronizadas.
- **La distorsión de audio durante los minijuegos** requiere configuración manual en FMOD Studio: los parámetros globales `distortion_aura`, `distortion_alberto` y `distortion_ruby` deben estar creados y conectados a efectos DSP en el evento de la canción. Sin esta configuración, el código es funcional pero no produce efecto audible.
- **El calibrado de la zona verde en TuningMinigame** depende de que Unity haya calculado el layout de la UI antes de ejecutar la corrutina de calibrado. Se resuelve esperando un frame (`yield return null`) antes de leer las dimensiones del `RectTransform`. En escenarios de carga con resoluciones no estándar, este calibrado podría requerir revisión.

### Simplificaciones por tratarse de una demo

- No se ha implementado persistencia de datos (guardado, puntuaciones históricas).
- No se han añadido pantallas de opciones ni configuración de audio.
- Los textos de UI están escritos directamente en el Inspector, sin sistema de localización.
- El sistema de pausa detiene el audio completo de FMOD, sin gestionar estados intermedios del evento (parámetros de transición en curso).

---

## 4.9 Resultado final

### Ejecución de la demo

La demo se ejecuta como build de escritorio para Windows, generada desde el editor de Unity mediante `File → Build Settings → Build`. El punto de entrada es la escena de menú principal, desde la que el jugador accede al resto de escenas.

Para ejecutar la demo en modo edición desde Unity, es necesario tener los bancos de FMOD Studio compilados (`File → Build` en FMOD Studio, versión 2.02.34) y copiados en la ruta configurada en los ajustes de FMOD for Unity del proyecto.

- **Resolución de la build:** — pendiente de confirmar por el autor —.
- **Flujo de escenas:** Menú principal → Cinemática → LoopSongs → Minijuego (— pendiente de confirmar por el autor —: orden exacto de navegación entre escenas).

### Capturas representativas

[CAPTURA: Menú principal de la demo con las opciones de navegación visibles]

[CAPTURA: Escena de cinemática con el panel de diálogo activo, la cámara virtual de Cinemachine y el Timeline en reproducción visible en el editor]

[CAPTURA: Vista de la escena LoopSongs con el entorno 3D, los colliders de ZonaCancion/ZonaPista y el panel de MusicUI con los sliders de pistas activos]

[CAPTURA: Inspector del componente MinigameManager con los cuatro CharacterData, los tres minijuegos y los canvas de UI asignados]

[CAPTURA: Vista de la escena de minijuego durante el juego, con las barras de estabilidad de los cuatro personajes, una burbuja activa sobre un personaje y el panel de un minijuego abierto]

[CAPTURA: Pantalla de resultados finales al completar la canción, con el estado de los cuatro personajes y la puntuación]

[CAPTURA: Evento de FMOD Studio con los ocho marcadores visibles en el timeline y las pistas de instrumento con las automatizaciones de parámetro]

---

## Cosas que necesito confirmar contigo antes de cerrar este documento

### Sección 4.4

- ¿Qué momentos concretos de "Ápice de Estrellas" están sincronizados con acciones visuales? (Por ejemplo: "el cambio de cámara ocurre en el segundo X", "el diálogo de Alberto aparece cuando entra el coro".)
- ¿Cuántas líneas de diálogo tiene la cinemática y qué personajes intervienen en ellas?

### Sección 4.5

- ¿Cuáles son las canciones implementadas en la escena LoopSongs, y qué pistas de instrumento tiene cada una en FMOD Studio?
- ¿Cuántas ZonaPista tiene cada ZonaCancion, y cuáles son sus `paramName` y `displayName` exactos?

### Sección 4.6

- ¿Cuál es el valor exacto de `eventDrain` de Alberto configurado en el Inspector?

### Sección 4.9

- ¿Cuál es la resolución objetivo de la build final?
- ¿Cuál es el orden exacto de navegación entre escenas desde el menú principal?
