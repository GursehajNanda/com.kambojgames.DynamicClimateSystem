using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlickerLight2D : MonoBehaviour
{ 
    [SerializeField]private float m_minIntensity = 0.5f;
    [SerializeField]private float m_maxIntensity = 1.5f;
    [SerializeField]private float m_flickerSpeed = 0.1f;

    private float m_noiseOffset;
    private float m_currentIntensity;

    public float  CurrentIntensity =>m_currentIntensity;

    void Awake()
    {
        m_noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float time = Time.time * m_flickerSpeed + m_noiseOffset;
        float noise = Mathf.PerlinNoise(time, 0f);
        m_currentIntensity = Mathf.Lerp(m_minIntensity, m_maxIntensity, noise);
    }

}
