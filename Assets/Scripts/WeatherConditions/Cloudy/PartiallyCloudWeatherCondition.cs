using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartiallyCloudWeatherCondition : WeatherCondition
{
    [SerializeField] private CloudController m_cloudController = new();


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
        m_cloudController.StopSpawn();

        RemoveWeather();
    }

}
