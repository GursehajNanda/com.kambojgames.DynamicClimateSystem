using UnityEngine;

public class CloudyWeatherCondition : WeatherCondition
{
    [Header("Cloudy")]
    [SerializeField] private float m_cloudStrength = 0.4f;
    private float m_currentCloudStrength;


    public override void Initialize()
    {
        base.Initialize();
    }

    public override void UpdateCondition()
    {
        if (!IsWeatherActive()) return;

        base.UpdateCondition();

        float newCloudStrength = EvaluateOverTime(m_currentCloudStrength, 0.0f);
        ClimateData.SetCloudStrength(newCloudStrength);


        if (ClimateData.IsRunningWeatherTypeWithBehaviour(WeatherType.Clear, WeatherBehaviour.None))
        {
            DisableWeather();
        }

    }


    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;

        m_currentCloudStrength = m_cloudStrength;
        ClimateData.SetCloudStrength(m_currentCloudStrength);
        ClimateData.RemoveRunningWeather(WeatherType.Clear);

    }

    protected override void OnWeatherEnd()
    {
        base.OnWeatherEnd();
        DisableWeather();
        ClimateData.SetCloudStrength(0.0f);
    }


    private void DisableWeather()
    {
        RemoveWeather(null);
        ClimateData.AddRunningWeather(WeatherType.Clear, WeatherBehaviour.None);
    }
}