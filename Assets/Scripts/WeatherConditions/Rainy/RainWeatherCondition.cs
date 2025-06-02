using UnityEngine;

public class RainWeatherCondition : WeatherCondition
{
    [Header("Rain")]
    [SerializeField] private float m_rainSimulationSpeed;
    [SerializeField] private float m_rainRateOverTime;
    [Range(0, 1)] [SerializeField] float m_probabilityofThunder = 0.3f;

    private ParticleSystem m_rainDrops;

    [Header("Weather Objects")]
    [SerializeField] private Weather m_rainWeather;
    [SerializeField] private Weather m_thunderWeather;

    public override void Initialize()
    {
        base.Initialize();
        GameObject rainFX = GameObject.FindGameObjectWithTag("Rain");
        if (rainFX)
        {
            m_rainDrops = rainFX?.GetComponent<ParticleSystem>();
            if (!m_rainDrops)
            {
                Debug.LogError("Rain partical system is missing");
            }

            m_rainDrops.Stop();
        }
        else
        {
            Debug.LogError("Could not find rain effect object");
        }

        if (m_thunderWeather)
        {
            m_thunderWeather.Initialize();
        }

    }

    public override void UpdateCondition()
    {
        if (!IsWeatherActive()) return;

        base.UpdateCondition();

        float probability = Random.Range(0f, 1.0f);
        if (probability <= m_probabilityofThunder)
        {
            AddThunder();
        }

        if (!IsConditionMet())
        {
            DisableWeather();
        }
    }

    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;
        m_rainDrops.Play();
        SetParticleEmissionSpeed(m_rainDrops, m_rainSimulationSpeed);
        SetParticleEmissionRate(m_rainDrops, m_rainRateOverTime);
        Debug.Log("Rain");
    }

    protected override void OnWeatherEnd()
    {
        base.OnWeatherEnd();
        DisableWeather();
    }


    private void AddThunder()
    {
        ClimateData.Instance.AddWeatherObject(m_thunderWeather);
    }

    private void DisableWeather()
    {
        RemoveWeather(m_rainWeather);
        m_rainDrops.Stop();
    }
}