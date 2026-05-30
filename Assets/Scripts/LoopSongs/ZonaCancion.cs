using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZonaCancion : MonoBehaviour
{
    [SerializeField]
    private EventReference musicEvent;

    [Tooltip("Duraci�n fade al entrar/salir")]
    [SerializeField]
    private float fadeDuration = 2f;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MusicManager.Instance.IniciarEvento(musicEvent);
            MusicUI.Instance.Mostrar(GetComponentsInChildren<ZonaPista>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MusicManager.Instance.DetenerEvento(fadeDuration);
            MusicUI.Instance.Ocultar();
        }
    }
}
