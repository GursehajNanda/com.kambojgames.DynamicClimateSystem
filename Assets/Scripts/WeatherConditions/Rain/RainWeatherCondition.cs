using UnityEngine;

public class RainWeatherCondition : WeatherCondition
{
    [Header("Rain")]
    [SerializeField] private float m_rainSimulationSpeed;
    [SerializeField] private float m_rainRateOverTime;

    private GameObject m_rainFX;
    private ParticleSystem m_rainDrops;


    public override void Initialize()
    {
        base.Initialize();
        m_rainFX = GameObject.FindGameObjectWithTag("Rain");
        if (m_rainFX)
        {
            m_rainFX.SetActive(false);
            m_rainDrops = m_rainFX?.GetComponent<ParticleSystem>();
            if (!m_rainDrops)
            {
                Debug.LogError("Rain partical system is missing");
            }
        }
        else
        {
            Debug.LogError("Could not find rain effect object");
        }
    }

    public override void UpdateCondition()
    {
        base.UpdateCondition();
    }

    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;
        m_rainFX.SetActive(true);
        SetParticleEmissionSpeed(m_rainDrops,m_rainSimulationSpeed);
        SetParticleEmissionRate(m_rainDrops,m_rainRateOverTime);
    }

    protected override void OnWeatherEnd()
    {
        m_rainFX.SetActive(false);
        RemoveWeather();
    }

}
