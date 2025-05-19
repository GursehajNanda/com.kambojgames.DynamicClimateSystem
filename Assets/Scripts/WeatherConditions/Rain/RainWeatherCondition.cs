using UnityEngine;

public class RainWeatherCondition : WeatherCondition
{
    [Header("Rain")]
    [SerializeField] private float m_rainSimulationSpeed;
    [SerializeField] private float m_rainRateOverTime;
    [Range(0, 1)] [SerializeField] float m_probabilityofThunder = 0.3f;

    private GameObject m_rainFX;
    private ParticleSystem m_rainDrops;

    [Header("Weather Objects")]
    [SerializeField] private Weather m_rainWeather;
    [SerializeField] private Weather m_thunderWeather;

    public override void Initialize()
    {
        base.Initialize();
        m_rainFX = GameObject.FindGameObjectWithTag("Rain");
        if (m_rainFX)
        {
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

        if(m_thunderWeather)
        {
            m_thunderWeather.Initialize();
        }

    }

    public override void UpdateCondition()
    {
        base.UpdateCondition();

        float probability = Random.Range(0f, 1.0f);
        if (probability <= m_probabilityofThunder)
        {
            AddThunder();
        }

        if (!IsConditionMet())
        {
            RemoveWeather(m_rainWeather);
        }
    }

    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;
        m_rainFX.SetActive(true);
        SetParticleEmissionSpeed(m_rainDrops,m_rainSimulationSpeed);
        SetParticleEmissionRate(m_rainDrops,m_rainRateOverTime);
        Debug.Log("Rain");
    }

    protected override void OnWeatherEnd()
    {
        base.OnWeatherEnd();
        RemoveWeather(m_rainWeather);
    }

    public override void DeactivateWeather()
    {
        m_rainFX.SetActive(false);
        m_thunderWeather.DeactivateWeather();
    }

    private void AddThunder()
    {
        ClimateData.Instance.AddWeatherObject(m_thunderWeather);
    }
}
