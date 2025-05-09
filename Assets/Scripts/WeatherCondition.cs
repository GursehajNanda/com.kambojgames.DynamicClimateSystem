using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine;


//How to use weather behaviour in calculating probability??
public abstract class WeatherCondition : MonoBehaviour
{
  
    [Tooltip("Weather type condition to run this weather")]
    [SerializeField] private WeatherType m_weatherTypeCondition;
    [Tooltip("Months to hit this Weather Behaviour")]
    [SerializeField] private WeatherMonths m_weatherMonths;
    [Tooltip("Please ensure total sum of probabilities for effects should be 1")]
    [SerializeField]private List<WeatherEffects> m_weatherEffects;
    [Tooltip("Effects that can be summoned during Weather Behaviour")]
    [SerializeField] protected List<KeyValuePair<string,ParticleSystem>> m_particlefx;
    [SerializeField] protected List<KeyValuePair<string, VisualEffect>> m_vfx;


    WeatherInterpolator m_weatherInterpolator = new();


    protected WeatherEffects CurrentWeatherEffect;
    protected ClimateData ClimateData;

  

    protected virtual void Start()
    {
        ClimateData = ClimateData.Instance;

        m_weatherInterpolator.OnWeatherSelected += OnWeatherSelected;
        m_weatherInterpolator.OnWeatherEnd += OnWeatherEnd;
    }


    protected virtual void OnValidate()
    {
        float totalProbability = 0;

        if (m_weatherEffects.Count != 0)
        {
            foreach (WeatherEffects weatherEffects in m_weatherEffects)
            {
                totalProbability += weatherEffects.Probability;
            }
        }

        if(totalProbability != 1.0f)
        {
            Debug.LogError("Please ensure total sum of probabilities for effects should be 1");
        }

    }

    private bool IsConditionMet()
    {
        Month CurrentMonth = (Month)ClimateData.Instance.GetDateTimeYearData().Month;

        if (!IsMonthInWeather(CurrentMonth)) return false;

        List<Weather> RunningWeather = ClimateData.Instance.GetCureentRunningWeather();

        if (RunningWeather.Count == 0) return false;

        foreach (Weather weather in RunningWeather)
        {
            if (m_weatherTypeCondition == weather.WeatherType)
            {
                return true;
            }
        }

        return false;
    }

    public void SelectWeatherEffect(float weatherStartTime, float weatherEndTime)
    {
      
        if (!IsConditionMet()) return;

   
        foreach (var weatherEffect in m_weatherEffects)
        {
            if (IsWithinProbabaility(weatherEffect))
            {
                List<Weather> RunningWeather = ClimateData.Instance.GetCureentRunningWeather();

                foreach (Weather weather in RunningWeather)
                {
                    if (m_weatherTypeCondition == weather.WeatherType)
                    {

                        if (weatherEffect.WeatherBehaviorCondition != WeatherBehaviour.None)
                        {
                            if (weatherEffect.WeatherBehaviorCondition != weather.GetCurrentWeatherBehaviour())
                            {
                                return;
                            }
                        }

                        CurrentWeatherEffect = weatherEffect;
                        CurrentWeatherEffect.OnWeatherEvent.Raise();

                        m_weatherInterpolator.StartWeather(weatherStartTime, weatherEndTime);
                        return;
                    }
                }
            }

        }

    }


    private bool IsWithinProbabaility(WeatherEffects weathereffect)
    {
        float randomProbability = Random.Range(0f, 1f);
        float currentProbability = 0f;

        currentProbability += weathereffect.Probability;
        if (randomProbability < currentProbability)
        {
            return true;
        }

        return false;
    }

    private bool IsMonthInWeather(Month month)
    {
        // Convert Month to WeatherMonths using bit shift
        WeatherMonths monthAsFlag = (WeatherMonths)(1 << ((int)month - 1));
        return (m_weatherMonths & monthAsFlag) != 0;
    }


    public bool IsWeatherActive()
    {
        return m_weatherInterpolator.IsWeatherActive() && CurrentWeatherEffect != null;
    }


    public WeatherBehaviour GetCurrentWeatherBehaviour()
    {
        if(IsWeatherActive())
        {
            return CurrentWeatherEffect.WeatherBehavior;
        }

        return WeatherBehaviour.None;
    }

    protected abstract void OnWeatherSelected();

    protected abstract void OnWeatherEnd();
   

   
}

[System.Serializable]
public class WeatherEffects
{
    [SerializeField] private string m_weatherKey;
    [SerializeField] private WeatherBehaviour m_weatherBehavior;
    [Tooltip("Weather behaviour condition to run the weather Effect ")]
    [SerializeField] private WeatherBehaviour m_weatherBehaviorCondition;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float m_probability;
    [SerializeField] private GameEvent m_onWeatherEvent;

    public string WeatherKey => m_weatherKey;
    public WeatherBehaviour WeatherBehavior => m_weatherBehavior;
    public WeatherBehaviour WeatherBehaviorCondition => m_weatherBehaviorCondition;
    public float Probability =>m_probability;
    public  GameEvent OnWeatherEvent => m_onWeatherEvent;
    
}

public enum WeatherBehaviour
{
    None,
    Normal,
    Moderate,
    Heavy
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

