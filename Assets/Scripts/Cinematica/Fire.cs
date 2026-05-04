using UnityEngine;

public class Fire : MonoBehaviour
{
    public ParticleSystem ps;
    public Light fireLight;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        fireLight = GetComponentInChildren<Light>();
    }

    void Update()
    {
        // Flickering simple con Perlin Noise
        float f = Mathf.PerlinNoise(Time.time * 6f, 0f);
        fireLight.intensity = Mathf.Lerp(1.5f, 3f, f);
    }

    public void Ignite()
    {
        ps.Play();
        fireLight.enabled = true;
    }

    public void Extinguish()
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        fireLight.enabled = false;
    }
}