using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudWeatherCondition : WeatherCondition
{
    private Timer m_clearWeatherTimer;
    [SerializeField] private CloudController m_cloudController = new();
    [Range(0,1)]
    [SerializeField] private float m_moderateCloudStrength;
    [Range(0, 1)]
    [SerializeField] private float m_heavyCloudStrength;

    private float m_cloudStrength = 0;

    protected override void Start()
    {
        base.Start();
        m_cloudController.Initialize();
    }

    private void Update()
    {
        m_cloudController.UpdateClouds();

      //  UpdateCloudStrength()
    }


    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;

        //if (CurrentWeatherEffect.WeatherKey == "PartiallyCloudy")
        //{
        //    m_cloudController.StartSpawn();
        //}
        //else if (CurrentWeatherEffect.WeatherKey == "ModerateCloudy")
        //{
        //    m_cloudStrength = m_moderateCloudStrength;
        //    ClimateData.SetCloudStrength(m_moderateCloudStrength);
        //}
        //else if (CurrentWeatherEffect.WeatherKey == "HeavyCloudy")
        //{
        //    m_cloudStrength = m_heavyCloudStrength;
        //    ClimateData.SetCloudStrength(m_heavyCloudStrength);
        //}
    }

    protected override void OnWeatherEnd()
    {
        //if (CurrentWeatherEffect != null)
        //{
        //    if (CurrentWeatherEffect.WeatherKey == "PartiallyCloudy")
        //    {
        //        m_cloudController.StopSpawn();
        //    }

        //    CurrentWeatherEffect = null;
        //}

        RemoveWeather();
    }



    //void UpdateCloudStrength(float currentTime)
    //{
    //    if (m_cloudStrength == 0) return;

    //    if (currentTime < WeatherStartTime)
    //        return;

    //    if (currentTime >= WeatherEndTime)
    //    {
    //        m_cloudStrength = 0;
    //        ClimateData.SetCloudStrength(m_cloudStrength);
    //        ClearWeather();
    //        return;
    //    }

    //    float duration = WeatherEndTime - WeatherStartTime;
    //    float elapsed = currentTime - WeatherStartTime;
    //    float t = elapsed / duration;

    //    float currentStrength = Mathf.Lerp(m_cloudStrength, 0f, t);
    //    ClimateData.SetCloudStrength(currentStrength);
    //}
}
