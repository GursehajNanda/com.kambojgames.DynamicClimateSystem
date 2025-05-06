using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine;
using KambojGames.Utilities2D.Attributes;

//How to use weather behaviour in calculating probability??
public abstract class WeatherCondition : MonoBehaviour
{
    [Tooltip("Weather type condition to run this weather")]
    [SerializeField] private WeatherType m_weatherTypeCondition;
    [Tooltip("Please ensure total sum of probabilities for effects should be 1")]
    [SerializeField]private List<WeatherEffects> m_weatherEffects;
    [Tooltip("Range should be in hours and 24 hour format,this will not be real time but game time")]
    [SerializeField]private MinMaxFloat m_timeRangeToClearWeatherEffect;

    private bool m_isWeatherCleared;
    private bool m_isRunningWeather;

    protected WeatherEffects CurrentWeatherEffect;
    protected Timer ClearWeatherTimer;



    private void OnValidate()
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

        if(m_timeRangeToClearWeatherEffect.min < 0)
        {
            Debug.LogError("Minimum value of clearing weather effect cannot be 0");
        }
    }

    protected bool IsConditionMet()
    {
        List<Weather> RunningWeather = ClimateData.Instance.GetCureentRunningWeather();

        if (RunningWeather.Count == 0) return false;

        foreach (Weather weather in RunningWeather)
        {
            if (m_weatherTypeCondition == weather.WeatherType)
            {
                return true;
            }
        }

        ClearWeatherEffect();

        return false;
    }

    private  bool SelectWeatherEffect()
    {
        if (!IsConditionMet()) return false;


        float randomProbability = Random.Range(0f, 1f);
        float currentProbability = 0f;

        foreach (var weathereffect in m_weatherEffects)
        {
            currentProbability += weathereffect.Probability;

            if (randomProbability < currentProbability)
            {
                List<Weather> RunningWeather = ClimateData.Instance.GetCureentRunningWeather();

                foreach (Weather weather in RunningWeather)
                {
                    if (m_weatherTypeCondition == weather.WeatherType)
                    {
                        
                        if (weathereffect.WeatherBehaviorCondition != WeatherBehaviour.None)
                        {
                            if(weathereffect.WeatherBehaviorCondition != weather.GetCurrentWeatherBehaviour())
                            {
                                return false;
                            }
                        }

                        CurrentWeatherEffect = weathereffect;
                        CurrentWeatherEffect.OnWeatherEvent.Raise();
                        return true;
                    }
                }

               
            }

        }

        return false;
    }


    public bool IsRunningWeatherEffect()
    {
        return m_isRunningWeather;
    }

    protected void ClearWeatherEffect()
    {
        if (!IsRunningWeatherEffect()) return;
        foreach (WeatherEffects effects in m_weatherEffects)
        {
            if (effects.VFX != null)
            {
                effects.VFX.Stop();
            }

            if (effects.Paricles != null)
            {
                effects.Paricles.Stop();
            }

        }

        m_isWeatherCleared = true;
        m_isRunningWeather = false;
        ClearWeatherTimer.Stop();
    }

    //Do not call StartWeatherEffect directly make another function may be Update Weather Effect, then do all the things you want to do?
    //May be do things in update?
    //Do need to call start weather effect, call it in the update

    public bool StartWeatherEffect()
    {
        //what if there is no weather effect?

        if (IsRunningWeatherEffect()) return false;

        if (SelectWeatherEffect())
        {
            CurrentWeatherEffect.VFX.Play();
            CurrentWeatherEffect.Paricles.Play();

            m_isRunningWeather = true;
            m_isWeatherCleared = false;
            return true;
        }
        return false;
    }



    public bool GetIsWeatherCleared()
    {
        return m_isWeatherCleared;
    }

    public WeatherBehaviour GetCurrentWeatherBehaviour()
    {
        if(IsRunningWeatherEffect())
        {
            return CurrentWeatherEffect.WeatherBehavior;
        }

        return WeatherBehaviour.None;
    }

    public abstract void StartClearingWeather();
}

[System.Serializable]
public struct WeatherEffects
{
    [SerializeField] private  string EffectTitle;
    [SerializeField] private ParticleSystem m_particles;
    [SerializeField] private VisualEffect m_vFX;
    [SerializeField] private WeatherBehaviour m_weatherBehavior;
    [Tooltip("Weather behaviour condition to run the weather Effect ")]
    [SerializeField] private WeatherBehaviour m_weatherBehaviorCondition;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float m_probability;
    [SerializeField] private GameEvent m_onWeatherEvent;
    //Months to run this weather??

    public ParticleSystem Paricles => m_particles;
    public VisualEffect VFX=> m_vFX;
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


