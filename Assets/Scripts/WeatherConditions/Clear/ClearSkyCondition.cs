using UnityEngine;

public class ClearSkyCondition : WeatherCondition
{
  
    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;
        ClimateData.SetCloudStrength(0.0f);
        Debug.Log("Clear Sky");
    }

    protected override void OnWeatherEnd(){}

}
