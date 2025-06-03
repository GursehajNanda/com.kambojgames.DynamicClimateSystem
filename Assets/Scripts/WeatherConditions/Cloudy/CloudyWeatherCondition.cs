using UnityEngine;

public class CloudyWeatherCondition : WeatherCondition
{
    [Header("Cloudy")]
    [SerializeField] private float m_cloudStrength = 0.4f;
    [Range(0, 1)] [SerializeField] float m_probabilityofRain = 0.3f;
    private float m_currentCloudStrength;

    [Header("Weather Objects")]
    [SerializeField] private Weather m_cloudWeather;
    [SerializeField] private Weather m_rainWeather;
    private Timer m_rainStartTimer = new Timer(0.0f, 0.0f, null, null);

    public override void Initialize()
    {
        base.Initialize();
        if (m_rainWeather)
        {
            m_rainWeather.Initialize();
        }


    }

    public override void UpdateCondition()
    {
        if (!IsWeatherActive()) return;

        base.UpdateCondition();

        float newCloudStrength = EvaluateOverTime(m_currentCloudStrength, 0.0f);
        ClimateData.SetCloudStrength(newCloudStrength);

        m_rainStartTimer.Update(Time.deltaTime);

        if (ClimateData.IsRunningWeatherTypeWithBehaviour(WeatherType.Clear, WeatherBehaviour.None))
        {
            RemoveWeather(m_cloudWeather);
        }

    }


    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;

        m_currentCloudStrength = m_cloudStrength;
        ClimateData.SetCloudStrength(m_currentCloudStrength);
        ClimateData.RemoveRunningWeather(WeatherType.Clear);
        float probability = Random.Range(0f, 1.0f);
        if (probability <= m_probabilityofRain)
        {
            float rainStartTime = Random.Range(0, WeatherEndTime - WeatherStartTime);

            m_rainStartTimer = new Timer(1.0f, rainStartTime, null, AddRain);
            m_rainStartTimer.Start();
            Debug.Log("Rain Will Start In: " + rainStartTime + "Seconds");
        }

        
    }

    protected override void OnWeatherEnd()
    {
        base.OnWeatherEnd();
        DisableWeather();
        ClimateData.SetCloudStrength(0.0f);
    }


    private void AddRain()
    {
        ClimateData.Instance.AddWeatherObject(m_rainWeather);
    }

    private void DisableWeather()
    {
        RemoveWeather(m_cloudWeather);
        ClimateData.AddRunningWeather(WeatherType.Clear, WeatherBehaviour.None);
    }
}