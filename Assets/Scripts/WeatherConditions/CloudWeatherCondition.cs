using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudWetherCondition : WeatherCondition
{

    private void Update()
    {
       if(IsConditionMet())
       {
            //Get Current Weather behaviour type and run code based on that?
       }
      
       //StarWeatherEffect
    }

    
    public override void StartClearingWeather()
    {
        //Start Clearing Weather
    }
}
