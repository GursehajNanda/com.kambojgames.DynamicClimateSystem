using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Weather", menuName = "ScriptableObject/WeatherData")]
public class Weather : ScriptableObject
{
    [SerializeField] private WeatherType m_weatherType;
    [SerializeField] private List<WeatherCondition> m_weatherConditions;
    [Tooltip("Time range  to check the probability of hitting the weather effect. Range should be in hours and 24 hour format, this will not be real time but game time.")]
    [SerializeField] private MinMaxFloat m_weatherTimeRange;

    private Timer m_weatherCoolDownTimer; //after selecting weather add a cooldown value
    private float m_weatherStartTime;
    private float m_weatherEndTime;

    public WeatherType WeatherType => m_weatherType;
    public MinMaxFloat WeatherTimeRange => m_weatherTimeRange;

   

    private void OnValidate()
    {
        if(m_weatherTimeRange.min <0)
        {
            Debug.LogError("Minimum value of Weather Time Range cannot be 0");
        }

        float totalProbability = 0;

        if (m_weatherConditions.Count != 0)
        {
            foreach (WeatherCondition conditions in m_weatherConditions)
            {
                totalProbability += conditions.Probability;
            }
        }

        if (totalProbability != 1.0f)
        {
            Debug.LogError("Please ensure total sum of probabilities for effects should be 1");
        }
    }

    private void StartWeatherEvent()
    {
        float MinutesToLastADay = ClimateData.Instance.MinutesToLastADay;
        float secondsPerInGameHour = (MinutesToLastADay / 24f) * 60f;
        float MinRealTimeInSeconds = WeatherTimeRange.min * secondsPerInGameHour;
        float MaxRealTimeInSeconds = WeatherTimeRange.max * secondsPerInGameHour;

        m_weatherStartTime = MinRealTimeInSeconds;
        m_weatherEndTime = MaxRealTimeInSeconds;

        WeatherCondition condition = GetWeatherConditionWithProbability();

        if(condition != null)
        {
            condition.SelectWeatherEffect(m_weatherType, m_weatherStartTime, m_weatherEndTime);

            //Start CoolDown
        }

    }

    public void UpdateWeather()
    {
        m_weatherCoolDownTimer.Update(Time.deltaTime);


        //if (!m_weatherConditions.IsWeatherActive())
        //{
        //    StartWeatherEvent();
        //}

    }

    private WeatherCondition GetWeatherConditionWithProbability()
    {
        float randomProbability = Random.Range(0f, 1f);
        float currentProbability = 0f;

        foreach (WeatherCondition condition in m_weatherConditions)
        {
            currentProbability += condition.Probability;
            if (randomProbability < currentProbability)
            {
                return condition;
            }
        }

        return null;
    }


}






