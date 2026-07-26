using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [SerializeField] private float minIntensity = 0.4f;
    [SerializeField] private float maxIntensity = 1.2f;
    [SerializeField] private float speed = 4f;

    private new Light light;
    private float seed;

    private void Start()
    {
        light = GetComponent<Light>();
        seed = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (light == null) return;

        var noise = Mathf.PerlinNoise(Time.time * speed, seed);
        light.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}
