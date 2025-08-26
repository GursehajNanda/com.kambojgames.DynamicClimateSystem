using UnityEngine;

public class RainWeatherCondition : WeatherCondition
{
    [Header("Rain")]
    [SerializeField] private float m_rainSimulationSpeed;
    [SerializeField] private float m_rainRateOverTime;
 

    private ParticleSystem m_rainDrops;


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


        OnWeatherConditionFailed += DisableWeather;

    }

    public override void UpdateCondition()
    {

        if (!IsWeatherActive()) return;

        base.UpdateCondition();
      
    }

    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;
        m_rainDrops.Play();
        SetParticleEmissionSpeed(m_rainDrops, m_rainSimulationSpeed);
        SetParticleEmissionRate(m_rainDrops, m_rainRateOverTime);
    }

    protected override void OnWeatherEnd()
    {
        base.OnWeatherEnd();
        DisableWeather();
    }


  

    private void DisableWeather()
    {
        RemoveWeather(null);
        m_rainDrops.Stop();
    }
}