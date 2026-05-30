using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string escenaMinijuego  = "Minijuego";
    [SerializeField] private string escenaEscenario  = "LoopSongs";
    [SerializeField] private string escenaCinematica = "Cinematica";

    [Header("Botones")]
    [SerializeField] private Button botonMinijuego;
    [SerializeField] private Button botonEscenario;
    [SerializeField] private Button botonCinematica;
    [SerializeField] private Button botonSalir;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        botonMinijuego?.onClick.AddListener(() => SceneManager.LoadScene(escenaMinijuego));
        botonEscenario?.onClick.AddListener(() => SceneManager.LoadScene(escenaEscenario));
        botonCinematica?.onClick.AddListener(() => SceneManager.LoadScene(escenaCinematica));
        botonSalir?.onClick.AddListener(Salir);
    }

    private void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
