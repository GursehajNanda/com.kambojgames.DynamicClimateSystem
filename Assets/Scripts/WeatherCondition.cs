using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine;
using KambojGames.Utilities2D;


public abstract class WeatherCondition : MonoBehaviour
{

    [SerializeField] private WeatherBehaviour m_weatherBehavior;

    [Header("Conditions")]
    [Tooltip("Weather type condition to run this weather")]
    [SerializeField] private WeatherTypeFlags m_weatherTypeCondition;
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
    protected float WeatherStartTime;
    protected float WeatherEndTime;

    public float Probability =>m_probability;

    public virtual void Initialize()
    {
        ClimateData = ClimateData.Instance;

        m_weatherInterpolator.OnWeatherSelected += OnWeatherSelected;
        m_weatherInterpolator.OnWeatherEnd += OnWeatherEnd;
    }


    public virtual void UpdateCondition()
    {
        m_weatherInterpolator.UpdateInterpolator();
    }

    public void SelectWeatherEffect(WeatherType weatherType, float weatherStartTime, float weatherEndTime)
    {

        if (!IsConditionMet()) return;

        WeatherStartTime = weatherStartTime;
        WeatherEndTime = weatherEndTime;
        m_weatherType = weatherType;

        ClimateData.AddRunningWeather(m_weatherType, m_weatherBehavior);
        m_weatherInterpolator.StartWeather(WeatherStartTime, WeatherEndTime);
        if (m_onWeatherEvent != null)
        {
            m_onWeatherEvent.Raise();
        }

    }


    public bool IsWeatherActive()
    {
        return m_weatherInterpolator.IsWeatherActive();
    }

    protected bool IsConditionMet()
    {
        Month CurrentMonth = (Month)ClimateData.GetDateTimeYearData().Month;

        if (!IsMonthInWeather(CurrentMonth)) return false;

        foreach (WeatherType type in (WeatherType[])System.Enum.GetValues(typeof(WeatherType)))
        {
            // Convert to bitmask only if not None
            WeatherTypeFlags flag = WeatherTypeFlags.None;
            if (type != WeatherType.None)
            {
                flag = (WeatherTypeFlags)(1 << ((int)type - 1));
            }

            if (m_weatherTypeCondition.HasFlag(flag))
            {
                if (ClimateData.IsRunningWeatherTypeWithBehaviour(type, m_weatherBehaviorCondition))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsMonthInWeather(Month month)
    {
        // Convert Month to WeatherMonths using bit shift
        WeatherMonths monthAsFlag = (WeatherMonths)(1 << ((int)month - 1));
        return (m_weatherMonths & monthAsFlag) != 0;
    }

    protected float EvaluateOverTime(float startValue, float endValue)
    {
        return m_weatherInterpolator.EvaluateOverTime(startValue, endValue);
    }

    protected void RemoveWeather(Weather weatherObject)
    {
        if (m_weatherType == WeatherType.None) return;
        ClimateData.RemoveRunningWeather(m_weatherType, m_weatherBehavior, weatherObject);
        Debug.Log("Removed Weather With Type: " + m_weatherType);
    }



    protected void SetParticleEmissionRate(ParticleSystem effect, float rate)
    {
        var emission = effect.emission;
        var rateOverTime = emission.rateOverTime;
        rateOverTime.constant = rate;
        emission.rateOverTime = rateOverTime;
    }

    protected void SetParticleEmissionSpeed(ParticleSystem effect, float speed)
    {
        var main = effect.main;
        main.simulationSpeed = speed;
    }

    protected void SetParticleStartColor(ParticleSystem effect, Color color)
    {
        color.a = 1;
        var main = effect.main;
        main.startColor = color;

    }

    protected virtual void OnWeatherEnd() { }
   

    protected abstract void OnWeatherSelected();

    public abstract void DeactivateWeather();
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
public enum WeatherTypeFlags
{
    None = 0,
    Clear = 1 << 0,
    Cloudy = 1 << 1,
    Rainy = 1 << 2,
    Windy = 1 << 3,
    Foggy = 1 << 4,
    Thunder = 1 << 5,
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

