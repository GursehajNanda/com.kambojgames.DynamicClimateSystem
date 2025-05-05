using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Weather",menuName = "ScriptableObject/WeatherData")]
public class Weather : ScriptableObject
{
    [SerializeField]private WeatherType m_weatherType;
    [SerializeField]private WeatherCondition m_weatherConditions;
    [SerializeField] private Vector2 m_weatherTimeRange; 

    public WeatherType WeatherType => m_weatherType;
    public Vector2 WeatherTimeRange => m_weatherTimeRange;

    public void SelectWeather()
    {
        m_weatherConditions.StartWeatherEffect();
    }
}






