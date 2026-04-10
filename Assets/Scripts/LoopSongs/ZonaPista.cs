using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZonaPista : MonoBehaviour
{
    [Tooltip("Nombre param FMOD")]
    [SerializeField] private string paramName;

    [Tooltip("Nombre UI")]
    [SerializeField] private string displayName;

    [Tooltip("Configuración Pista")]
    [SerializeField] public bool isStartingByStart = false;
    [SerializeField] public Color tintColor = Color.white;

    public string ParamName => paramName;
    public string DisplayName => displayName;

    public Color TintColor => tintColor;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MusicManager.Instance.EnterPista(this);
            ZoneShaderManager.Instance.SetTint(tintColor);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MusicManager.Instance.ExitPista(this);
            ZoneShaderManager.Instance.ClearTint();
        }
}
}
