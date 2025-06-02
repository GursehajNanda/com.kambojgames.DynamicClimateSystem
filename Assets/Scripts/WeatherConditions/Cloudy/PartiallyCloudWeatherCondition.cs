using UnityEngine;

public class PartiallyCloudWeatherCondition : WeatherCondition
{
    [SerializeField] private CloudController m_cloudController = new();

    [Header("Weather Objects")]
    [SerializeField] private Weather m_cloudWeather;

    public override void Initialize()
    {
        base.Initialize();
        m_cloudController.Initialize();
    }

    public override void UpdateCondition()
    {
        base.UpdateCondition();
        m_cloudController.UpdateClouds();

    }


    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;

        m_cloudController.StartSpawn();
        Debug.Log("Partially Cloudy");
    }

    protected override void OnWeatherEnd()
    {
        base.OnWeatherEnd();
        m_cloudController.StopSpawn();
        RemoveWeather(m_cloudWeather);
    }



}