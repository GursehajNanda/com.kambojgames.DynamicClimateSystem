using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine;
using KambojGames.Utilities2D;

//How to use weather behaviour in calculating probability??
public abstract class WeatherCondition : MonoBehaviour
{

    [SerializeField] private WeatherBehaviour m_weatherBehavior;

    [Header("Conditions")]
    [Tooltip("Weather type condition to run this weather")]
    [SerializeField] private WeatherType m_weatherTypeCondition;
    [Tooltip("Weather behaviour condition to run the weather Effect ")]
    [SerializeField] private WeatherBehaviour m_weatherBehaviorCondition;
    [Tooltip("Months to hit this Weather Behaviour")]
    [SerializeField] private WeatherMonths m_weatherMonths;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float m_probability;
  

    [Header("Effects")]
    [SerializeField] protected KeyValuePairList<string,ParticleSystem> m_particlefx = new();
    [SerializeField] protected KeyValuePairList<string, VisualEffect> m_vfx = new();

    [Header("Game Events")]
    [SerializeField] private GameEvent m_onWeatherEvent;

    private WeatherInterpolator m_weatherInterpolator = new();
    private WeatherType m_weatherType;

    protected ClimateData ClimateData;

    public float Probability =>m_probability;

    protected virtual void Start()
    {
        ClimateData = ClimateData.Instance;

        m_weatherInterpolator.OnWeatherSelected += OnWeatherSelected;
        m_weatherInterpolator.OnWeatherEnd += OnWeatherEnd;
    }



    private bool IsConditionMet()
    {
        Month CurrentMonth = (Month)ClimateData.GetDateTimeYearData().Month;

        if (!IsMonthInWeather(CurrentMonth)) return false;

        if(ClimateData.RunningWeather.IsRunningWeatherTypeWithBehaviour(m_weatherTypeCondition, m_weatherBehaviorCondition))
        {
            return true;
        }

        return false;
    }

    public bool SelectWeatherEffect(WeatherType weatherType,float weatherStartTime, float weatherEndTime)
    {
      
        if (!IsConditionMet()) return false;

        m_weatherType = weatherType;
        ClimateData.RunningWeather.AddWeather(m_weatherType, m_weatherBehavior);
        m_weatherInterpolator.StartWeather(weatherStartTime, weatherEndTime);
        m_onWeatherEvent.Raise();
        return true;
    }

    //Figure Out this
   

    private bool IsMonthInWeather(Month month)
    {
        // Convert Month to WeatherMonths using bit shift
        WeatherMonths monthAsFlag = (WeatherMonths)(1 << ((int)month - 1));
        return (m_weatherMonths & monthAsFlag) != 0;
    }


    public bool IsWeatherActive()
    {
        return m_weatherInterpolator.IsWeatherActive();
    }


    protected void RemoveWeather()
    {
        ClimateData.RunningWeather.AddWeather(m_weatherType, m_weatherBehavior);
    }

    protected abstract void OnWeatherSelected();

    protected abstract void OnWeatherEnd();
   

   
}



public enum WeatherType
{
    None,
    Clear,
    Cloudy,
    Rainy,
    Windy,
    Foggy,
    Thunder,
}

[System.Flags]
public enum WeatherMonths
{
    None = 0,
    January = 1 << 0,  
    February = 1 << 1,  
    March = 1 << 2,  
    April = 1 << 3,  
    May = 1 << 4,  
    June = 1 << 5,  
    July = 1 << 6,  
    August = 1 << 7,  
    September = 1 << 8,  
    October = 1 << 9,  
    November = 1 << 10, 
    December = 1 << 11  
}

