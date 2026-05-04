using UnityEngine;
using System.Collections;

public class Firework : MonoBehaviour
{
    public ParticleSystem trail;
    public ParticleSystem burst;

    [Header("Configuración")]
    public float riseSpeed = 8f;
    public float riseTime = 1.5f;   // segundos subiendo

    static Color[] colors = {
        Color.red, Color.cyan, Color.yellow,
        new Color(1f,0.4f,0f),   // naranja
        new Color(0.6f,0f,1f),   // morado
        Color.green, Color.white,
        new Color(1f,0.4f,0.8f)  // rosa
    };

    void Awake()
    {
        trail = transform.Find("Trail").GetComponent<ParticleSystem>();
        burst = transform.Find("Burst").GetComponent<ParticleSystem>();
    }

    void Start()
    {
        // Color aleatorio para este cohete
        Color c = colors[Random.Range(0, colors.Length)];
        SetColor(trail, c);
        SetColor(burst, c);

        trail.Play();
        StartCoroutine(Rise());
    }

    IEnumerator Rise()
    {
        float t = 0f;
        while (t < riseTime)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }

        // Explotar
        trail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        burst.Play();

        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    void SetColor(ParticleSystem ps, Color c)
    {
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(c, c * 0.6f);
    }
}