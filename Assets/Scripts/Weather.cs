using UnityEngine;
using System.Collections.Generic;
using KambojGames.Utilities2D.Attributes;

[CreateAssetMenu(fileName = "Weather", menuName = "ScriptableObject/WeatherData")]
public class Weather : ScriptableObject
{
    [SerializeField] private WeatherType m_weatherType;
    [SerializeField] private List<WeatherCondition> m_weatherConditions;
    [Tooltip("Time range  to finish weather effect, Range should be in hours and 24 hour format, this will not be real time but game time.")]
    [SerializeField] private MinMaxFloat m_weatherTimeRange;
    [Tooltip("Cool down time after which a weather selection is made should be in hours and 24 hour format, this will not be real time but game time")]
    [SerializeField] private float m_coolDownTime;

    private WeatherCondition ActiveWeatherCondition;
    private Timer m_weatherCoolDownTimer; 
    private float m_weatherStartTime;
    private float m_weatherEndTime;
    private ClimateData m_climateData;
    private bool m_isInCooldown;

    public WeatherType WeatherType => m_weatherType;




    private void OnValidate()
    {
        if(m_weatherTimeRange.min <0 && m_weatherTimeRange.max !=0)
        {
            Debug.LogError("Minimum value of Weather Time Range cannot be 0");
        }

        float totalProbability = 0;

        if (m_weatherConditions != null && m_weatherConditions.Count == 0) return;

        foreach (WeatherCondition condition in m_weatherConditions)
        {
            if (condition != null)
            {
                totalProbability += condition.Probability;
            }
        }

        if (totalProbability != 1.0f)
        {
            Debug.LogError("Please ensure total sum of probabilities for all Weather Conditions should be 1");
        }
    }

    public void Initialize()
    {
        m_climateData = ClimateData.Instance;
        ActiveWeatherCondition = null;
        m_isInCooldown = false;

        float coolDownRealTime = m_climateData.CovertGameHoursToRealTimeInSecs(m_coolDownTime);
        m_weatherCoolDownTimer = new Timer(1.0f, coolDownRealTime, null, StopCoolDown);

        foreach (WeatherCondition condition in m_weatherConditions)
        {
            condition.Initialize();
        }


    }

    public void ActivateWeather()
    {
       

        if (m_isInCooldown || ActiveWeatherCondition) return;

        m_weatherStartTime = Time.time;

        float weatherDurationInHours = Random.Range(m_weatherTimeRange.min, m_weatherTimeRange.max); 
        float weatherEndRealTimeInSeconds = m_climateData.CovertGameHoursToRealTimeInSecs(weatherDurationInHours);

        m_weatherEndTime = m_weatherStartTime + weatherEndRealTimeInSeconds;


        ActiveWeatherCondition = GetWeatherConditionWithProbability();

        if(ActiveWeatherCondition != null)
        {
            ActiveWeatherCondition.SelectWeatherEffect(m_weatherType, m_weatherStartTime, m_weatherEndTime);
        }

    }

    public void DisableWeather()
    {
       
        if (ActiveWeatherCondition)
        {
            ActiveWeatherCondition = null;
            m_isInCooldown = true;


            m_weatherCoolDownTimer.Reset();
            m_weatherCoolDownTimer.Start();
        }
    }

   
    public void UpdateWeather()
    {
        m_weatherCoolDownTimer.Update(Time.deltaTime);

        if (ActiveWeatherCondition)
        {
            ActiveWeatherCondition.UpdateCondition();
        }

    }

    public void StopCoolDown()
    {
        m_isInCooldown = false;
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






