using UnityEngine;
using KambojGames.Utilities2D.Animator;
using System.Collections.Generic;

public class WeatherUIController : MonoBehaviour
{
    [SerializeField] private List<CustomAnimationClip> m_animationClips;
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    [SerializeField] private AudioSource m_lightRain;
    [SerializeField] private AudioSource m_heavyRain;
    [SerializeField] private AudioSource m_thunder;

    private CharacterAnimator m_weatherAnimation;
    private DayPeriod m_currentDayPeriod;

    private void Start()
    {
        m_weatherAnimation = new CharacterAnimator(m_spriteRenderer);

        foreach (CustomAnimationClip clip in m_animationClips)
        {
            m_weatherAnimation.AddAnimation(clip, clip.AnimationFrames.Count * 2.0f);
        }
        UpdateUI();
    }
    void Update()
    {
        m_weatherAnimation.HandleUpdateForAnimation();

        UpdateUI();
    }

    private void UpdateUI()
    {
        StopRain();
        StopThunder();


        if (IsRunningWeather(WeatherTypeFlags.Clear))
        {
            StopWeatherEffects();
        }
        else
        {
            HandleWeatherAnimations();
            HandleWeatherEffects();
        }

    }

    void StopWeatherEffects()
    {
        StopRain();
        StopThunder();

        DayPeriod dayPeriod = ClimateData.Instance.GetDayPeriod();

        if (m_currentDayPeriod != dayPeriod)
        {
            m_currentDayPeriod = dayPeriod;
            UpdateDayPeriodAnimation();
        }
    }

    void HandleWeatherAnimations()
    {
        DayPeriod dayPeriod = ClimateData.Instance.GetDayPeriod();

        if (IsRunningWeather(WeatherTypeFlags.Rainy))
        {
            m_weatherAnimation.UpdateAnimationState("Rain");
        }
        else if (IsRunningWeather(WeatherTypeFlags.Thunder))
        {
            m_weatherAnimation.UpdateAnimationState("Thunder");
        }
        else if (m_currentDayPeriod != dayPeriod) 
        {
            m_currentDayPeriod = dayPeriod;
            UpdateDayPeriodAnimation();
        }
    }

    void HandleWeatherEffects()
    {
        if (IsRunningWeather(WeatherTypeFlags.Rainy))
        {
            WeatherBehaviour weatherBehaviour = ClimateData.Instance.GetCurrentWeatherBehaviour(WeatherType.Rainy);

            if (weatherBehaviour == WeatherBehaviour.Normal || weatherBehaviour == WeatherBehaviour.Moderate)
            {
                if (m_lightRain.isPlaying) return;
                    m_lightRain.Play();
            }
            else if (weatherBehaviour == WeatherBehaviour.Heavy)
            {
                if (m_heavyRain.isPlaying) return;
                m_heavyRain.Play();
            }
        }
        else if (IsRunningWeather(WeatherTypeFlags.Thunder))
        {
            m_thunder.Play();
        }
    }

    void UpdateDayPeriodAnimation()
    {
        switch (m_currentDayPeriod)
        {
            case DayPeriod.Morning:
                m_weatherAnimation.UpdateAnimationState("Morning");
                break;
            case DayPeriod.Afternoon:
                m_weatherAnimation.UpdateAnimationState("Afternoon");
                break;
            case DayPeriod.Evening:
                m_weatherAnimation.UpdateAnimationState("Evening");
                break;
            case DayPeriod.Night:
                m_weatherAnimation.UpdateAnimationState("Night");
                break;
        }
    }

    private bool IsRunningWeather(WeatherTypeFlags weatherFlags)
    {
        if( ClimateData.Instance.IsRunningWeatherTypeWithBehaviour(weatherFlags))
        {
            return true;
        }

        return false;
    }

    private void StopRain()
    {
        if (!IsRunningWeather(WeatherTypeFlags.Rainy))
        {
            m_lightRain.Stop();
            m_heavyRain.Stop();
        }
    }

    private void StopThunder()
    {
        if (!IsRunningWeather(WeatherTypeFlags.Thunder))
        {
            m_thunder.Stop();
        }
        
    }
}
