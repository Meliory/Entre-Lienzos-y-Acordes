using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public enum TipoEscena { Minijuego, Cinematica, LoopSongs }

    [Header("Tipo de escena")]
    public TipoEscena tipoEscena;

    [Header("Panel de pausa")]
    public GameObject panelPausa;
    public TextMeshProUGUI cuentaAtrasText;

    [Header("Botones")]
    public Button botonReanudar;
    public Button botonReiniciar;
    public Button botonMenuPrincipal;

    [Header("Escenas")]
    public string nombreEscenaMenuPrincipal = "MainMenu";

    [Header("Referencias (según escena)")]
    public MinigameManager   minigameManager;
    public CinematicaManager cinematicaManager;
    public MusicManager      musicManager;
    public CameraController  cameraController;

    private bool _pausado;

    // ================================================================== //
    //  Unity lifecycle
    // ================================================================== //

    private void Start()
    {
        panelPausa?.SetActive(false);
        if (cuentaAtrasText) cuentaAtrasText.gameObject.SetActive(false);

        botonReiniciar?.gameObject.SetActive(tipoEscena != TipoEscena.LoopSongs);

        botonReanudar?.onClick.AddListener(BotonReanudar);
        botonReiniciar?.onClick.AddListener(BotonReiniciar);
        botonMenuPrincipal?.onClick.AddListener(BotonMenuPrincipal);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (tipoEscena == TipoEscena.Minijuego
            && (minigameManager == null || minigameManager.IsPartidaTerminada)) return;

        AlternarPausa();
    }

    // ================================================================== //
    //  Lógica de pausa
    // ================================================================== //

    private void AlternarPausa()
    {
        if (_pausado) BotonReanudar();
        else          Pausar();
    }

    private void Pausar()
    {
        _pausado = true;
        panelPausa?.SetActive(true);
        MostrarCursor(true);

        switch (tipoEscena)
        {
            case TipoEscena.Minijuego:
                minigameManager?.PausarDesdeMenu();
                break;

            case TipoEscena.Cinematica:
                Time.timeScale = 0f;
                cinematicaManager?.Pausar();
                break;

            case TipoEscena.LoopSongs:
                Time.timeScale = 0f;
                musicManager?.SetPausado(true);
                if (cameraController != null) cameraController.enabled = false;
                break;
        }
    }

    // ================================================================== //
    //  Botones
    // ================================================================== //

    public void BotonReanudar()
    {
        if (!_pausado) return;
        panelPausa?.SetActive(false);

        switch (tipoEscena)
        {
            case TipoEscena.Minijuego:
                StartCoroutine(ReanudarConCuentaAtras());
                return;

            case TipoEscena.Cinematica:
                Time.timeScale = 1f;
                cinematicaManager?.Reanudar();
                break;

            case TipoEscena.LoopSongs:
                Time.timeScale = 1f;
                musicManager?.SetPausado(false);
                if (cameraController != null) cameraController.enabled = true;
                MostrarCursor(false);
                break;
        }

        _pausado = false;
    }

    private IEnumerator ReanudarConCuentaAtras()
    {
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
        minigameManager?.ReanudarDesdeMenu();
    }

    public void MostrarMenuFinal()
    {
        panelPausa?.SetActive(true);
        botonReanudar?.gameObject.SetActive(false);
    }

    public void BotonReiniciar()
    {
        Time.timeScale = 1f;
        MostrarCursor(tipoEscena != TipoEscena.LoopSongs);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BotonMenuPrincipal()
    {
        Time.timeScale = 1f;
        MostrarCursor(true);
        SceneManager.LoadScene(nombreEscenaMenuPrincipal);
    }

    // ================================================================== //

    private static void MostrarCursor(bool mostrar)
    {
        Cursor.lockState = mostrar ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible   = mostrar;
    }
}
