using UnityEngine;

public class FireworksLauncher : MonoBehaviour
{
    public GameObject fireworkPrefab;
    public float interval = 0.8f;      // segundos entre cohetes
    public float spread = 5f;          // dispersión horizontal

    void Start() => InvokeRepeating(nameof(Launch), 0f, interval);

    void Launch()
    {
        Vector3 pos = transform.position + new Vector3(
            Random.Range(-spread, spread), 0f,
            Random.Range(-spread, spread));

        Instantiate(fireworkPrefab, pos, Quaternion.identity);
    }
}