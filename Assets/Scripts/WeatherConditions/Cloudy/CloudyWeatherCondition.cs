using UnityEngine;

public class CloudyWeatherCondition : WeatherCondition
{
    [SerializeField] private float m_cloudStrength = 0.4f;

    private float m_currentCloudStrength;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void UpdateCondition()
    {
        base.UpdateCondition();

        if (!IsWeatherActive()) return;
        float newCloudStrength = EvaluateOverTime(m_currentCloudStrength, 0.0f);
        ClimateData.SetCloudStrength(newCloudStrength);
    }


    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;
        m_currentCloudStrength = m_cloudStrength;
        ClimateData.SetCloudStrength(m_currentCloudStrength);
        ClimateData.RunningWeather.RemoveWeather(WeatherType.Clear, WeatherBehaviour.None);
        Debug.Log("Moderate Cloudy");
    }

    protected override void OnWeatherEnd()
    {
        ClimateData.SetCloudStrength(0.0f);
        RemoveWeather();
        ClimateData.RunningWeather.AddWeather(WeatherType.Clear, WeatherBehaviour.None);
    }
}
