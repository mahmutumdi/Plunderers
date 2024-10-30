using UnityEngine;

public class GlowPulse : MonoBehaviour
{
    private Material glowMaterial;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minGlowIntensity = 0.5f;
    [SerializeField] private float maxGlowIntensity = 2f;

    void Start()
    {
        glowMaterial = GetComponent<Renderer>().material;
        glowMaterial.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        float emission = Mathf.Lerp(minGlowIntensity, maxGlowIntensity, Mathf.PingPong(Time.time * pulseSpeed, 1f));
        glowMaterial.SetColor("_EmissionColor", glowMaterial.color * emission);
    }
}