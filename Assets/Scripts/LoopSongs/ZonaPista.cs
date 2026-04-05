using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZonaPista : MonoBehaviour
{
    [Tooltip("Nombre param FMOD")]
    [SerializeField] 
    private string paramName;

    [Tooltip("Nombre UI")]
    [SerializeField]
    private string displayName;

    public string ParamName => paramName;
    public string DisplayName => displayName;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MusicManager.Instance.EnterPista(this);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MusicManager.Instance.ExitPista(this);
        }
}
}
