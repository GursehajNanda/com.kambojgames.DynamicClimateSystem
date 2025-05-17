using UnityEngine;
using System.Collections.Generic;

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

    public WeatherType WeatherType => m_weatherType;
   

   

    private void OnValidate()
    {
        if(m_weatherTimeRange.min <0)
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

        float coolDownRealTime = CovertGameHoursToRealTimeInSecs(m_coolDownTime);
        m_weatherCoolDownTimer = new Timer(1.0f, coolDownRealTime, null, StartWeatherEvent);
        m_weatherCoolDownTimer.Start();

        ActiveWeatherCondition = null;

        foreach (WeatherCondition condition in m_weatherConditions)
        {
            condition.Initialize();
        }


    }

    private void StartWeatherEvent()
    {
        m_weatherStartTime = Time.time;

        float weatherDurationInHours = Random.Range(m_weatherTimeRange.min, m_weatherTimeRange.max); 
        float weatherEndRealTimeInSeconds = CovertGameHoursToRealTimeInSecs(weatherDurationInHours);

        m_weatherEndTime = m_weatherStartTime + weatherEndRealTimeInSeconds;

        Debug.Log("Weather Start Time: " + m_weatherStartTime);
        Debug.Log("Weather End Time: " + m_weatherEndTime);

        ActiveWeatherCondition = GetWeatherConditionWithProbability();

        if(ActiveWeatherCondition != null)
        {
            ActiveWeatherCondition.SelectWeatherEffect(m_weatherType, m_weatherStartTime, m_weatherEndTime);
        }

    }

    public void UpdateWeather()
    {
      
        m_weatherCoolDownTimer.Update(Time.deltaTime);

        if (!m_weatherCoolDownTimer.IsTimerRunning())
        {
            if (ActiveWeatherCondition != null && !ActiveWeatherCondition.IsWeatherActive())
            {
                m_weatherCoolDownTimer.Reset();
                m_weatherCoolDownTimer.Start();
                ActiveWeatherCondition = null;
            }
        }

        if (ActiveWeatherCondition)
        {
            ActiveWeatherCondition.UpdateCondition();
        }
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

    private float CovertGameHoursToRealTimeInSecs(float time)
    {
        float minutesToLastADay = m_climateData.MinutesToLastADay;
        float secondsPerInGameHour = (minutesToLastADay / 24f) * 60f;
        return( time * secondsPerInGameHour);
    }

}






