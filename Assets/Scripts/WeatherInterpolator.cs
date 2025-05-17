using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherInterpolator
{
    public System.Action OnWeatherSelected;
    public System.Action OnWeatherEnd;

    private float m_weatherStartTime;
    private float m_weatherEndTime;
    private float m_elapsedTime;
    private bool m_weatherActive;

    public void StartWeather(float startTime, float endTime)
    {
        m_weatherStartTime = startTime;
        m_weatherEndTime = endTime;
        m_elapsedTime = 0f;
        m_weatherActive = true;

        OnWeatherSelected.Invoke();
    }

    public void UpdateInterpolator()
    {
        if (!m_weatherActive)
            return;

        m_elapsedTime += Time.deltaTime;

        if (m_elapsedTime >= m_weatherEndTime)
        {
            m_weatherActive = false;
            OnWeatherEnd?.Invoke();
        }
    }

    public float EvaluateOverTime(float startValue, float endValue)
    {
        if (IsWeatherActive())
        {
            float t = Mathf.InverseLerp(m_weatherStartTime, m_weatherEndTime, m_elapsedTime);
            return Mathf.Lerp(startValue, endValue, t);
        }
        return 0.0f;
    }

  
    public bool IsWeatherActive()
    {
        return m_weatherActive;
    }

    public void StopInterpolator()
    {
        m_weatherActive = false;
    }
}
