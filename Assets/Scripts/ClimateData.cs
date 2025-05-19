using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(fileName = "ClimateData", menuName = "ScriptableObject/ClimateData")]
public class ClimateData : ScriptableObject
{
    [Header("Date Time And Day Data")]
    [Min(1)]
    [SerializeField] private int m_year;
    [SerializeField] private Month m_month;
    [Range(1, 31)]
    [SerializeField] private int m_monthDay;
    [Range(0, 23)]
    [SerializeField] private int m_hour;
    [Range(1, 59)]
    [SerializeField] private int m_minute;
    [SerializeField] [Min(1.0f)] private float m_minutesToLastADay;


    [Header("Seasonal Day Light")]
    [SerializeField] private Gradient m_summerColorGradient;
    [SerializeField] private Gradient m_winterColorGradient;
    [SerializeField] private Gradient m_springColorGradient;
    [SerializeField] private Gradient m_autumnColorGradient;

    [Header("Weather")]
    [SerializeField] List<Weather> m_weatherObjects;

    private float m_cloudsStrength;
    private DayPeriod m_dayPeriod;
    private Season m_currentSeason;
    private static ClimateData m_instance;
    private Material m_seasonMaterial;
    private Material m_seasonVegetationMaterial;
    private List<ShadowInstance> m_shadows = new();
    private List<LightInterpolator> m_lightBlender = new();
    private RunningWeather m_runningWeather = new();

    public float MinutesToLastADay => m_minutesToLastADay;
    public Gradient SummerColorGradient => m_summerColorGradient;
    public Gradient WinterColorGradient => m_winterColorGradient;
    public Gradient SpringColorGradient => m_springColorGradient;
    public Gradient AutumnColorGradient => m_autumnColorGradient;

    public List<ShadowInstance> Shadows => m_shadows;
    public List<LightInterpolator> LightBlender => m_lightBlender;

    public float CloudsStrength => m_cloudsStrength;
   

    private void OnValidate()
    {
        // Clamp the year
        if (m_year < 1)
            m_year = 1;

        // Get max valid days for the current month & year
        int maxDays = DateTime.DaysInMonth(m_year, (int)m_month);

        // Clamp the day based on the actual number of days in the month
        if (m_monthDay > maxDays)
            m_monthDay = maxDays;
        else if (m_monthDay < 1)
            m_monthDay = 1;
    }

    public static ClimateData Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = Resources.Load<ClimateData>("ClimateData");
                if(m_instance == null)
                {
                    Debug.LogError("ClimateData instance not found in Resources.");
                }

                m_instance.Initialize();
            }
            return m_instance;
        }
    }

    private void Initialize()
    {

        m_seasonMaterial = Resources.Load<Material>("Materials/SeasonalTint_lit");
        m_seasonVegetationMaterial = Resources.Load<Material>("Materials/SeasonalVegetationMaterial");
        m_cloudsStrength = 0;

        foreach (Weather weather in m_weatherObjects)
        {
            weather.Initialize();
        }

        foreach (Weather weather in m_weatherObjects)
        {
            weather.DeactivateWeather();
        }

        AddRunningWeather(WeatherType.Clear, WeatherBehaviour.None);
    }


    public void UpdateWeather()
    {

        for (int i = 0; i < m_weatherObjects.Count; i++)
        {
            m_weatherObjects[i].UpdateWeather();
        }
   

        if(Input.GetKeyDown(KeyCode.Return))
        {
            Dictionary<WeatherType, WeatherBehaviour> runningWeather = m_runningWeather.GetRunningWeather();

            foreach (KeyValuePair<WeatherType, WeatherBehaviour> kvp in runningWeather)
            {
                Debug.Log("Running Weather is" + $"Key: {kvp.Key}, Value: {kvp.Value}");
            }
        }
    }

    public void AddWeatherObject(Weather weatherObject)
    {
        if (weatherObject != null)
        {
            if (!m_weatherObjects.Contains(weatherObject))
            {
                m_weatherObjects.Add(weatherObject);
            }
        }
    }

    public void AddRunningWeather(WeatherType type, WeatherBehaviour behaviour)
    {
        m_runningWeather.AddWeather(type, behaviour);
       
        UpdateWeatherConditions();
    }

    public void RemoveRunningWeather(WeatherType type, WeatherBehaviour behaviour, Weather weatherObject = null)
    {     
        foreach(Weather weather in m_weatherObjects)
        {
            if(weather.WeatherType == type)
            {
                weather.DeactivateWeather();
            }
        }

        m_runningWeather.RemoveWeather(type, behaviour);

        if (weatherObject != null)
        {
            m_weatherObjects.Remove(weatherObject);
        }

        UpdateWeatherConditions();
    }

    private void UpdateWeatherConditions()
    {
        foreach (Weather weather in m_weatherObjects)
        {
            weather.ActivateWeather();
        }
    }

    public bool IsRunningWeatherTypeWithBehaviour(WeatherType type, WeatherBehaviour behaviour)
    {
        return m_runningWeather.IsRunningWeatherTypeWithBehaviour(type, behaviour);
    }

    public void SetCloudStrength(float value)
    {
        if(value <0 || value >1)
        {
            Debug.Log("Cloud strength value must be between 0 and 1");
        }
        m_cloudsStrength = value;

    }

    public void SetYear(int Year)
    {
        if (Year < 1)
        {
            m_year = 1;
        }
        else
        {
            m_year = Year;
        }
       
    }

    public void SetMonth(Month month)
    {
        m_month = month;
    }

    public void SetMonthDay(int monthDay)
    {
        m_monthDay = Mathf.Clamp(monthDay, 1, DateTime.DaysInMonth(m_year, (int)m_month));
    }

    public void SetHourOfDay(int hour)
    {
        m_hour = Mathf.Clamp(hour, 0, 23);
    }

    public void SetMinuteOfHour(int minute)
    {
        m_minute = Mathf.Clamp(minute, 0, 59); ;
    }

    public void SetDayPeriod(DayPeriod dayPeriod)
    {
        m_dayPeriod = dayPeriod;
    }

    public void  SetDateTimeYearData(DateTime dateTimeYearData)
    {
        m_year = dateTimeYearData.Year;
        m_month = (Month)(dateTimeYearData.Month);
        m_monthDay = dateTimeYearData.Day;
        m_hour = dateTimeYearData.Hour;
        m_minute = dateTimeYearData.Minute;
    }

    public void SetCurrentSeason(Season season)
    {
        m_currentSeason = season;
    }

    public void SetMaterialSesaonalBlend(float blendFactor)
    {
       m_seasonMaterial.SetFloat("_BlendFactor", blendFactor);
       m_seasonVegetationMaterial.SetFloat("_BlendFactor", blendFactor);
       
    }

    public Color GetVegetationColor()
    {
        Color baseColor = Color.green;
        baseColor.a = 1;
        switch (GetCurrentSeason())
        {
            case Season.Autumn:
                baseColor = m_seasonVegetationMaterial.GetColor("_AutumnTint");
                break;
            case Season.Spring:
                baseColor = m_seasonVegetationMaterial.GetColor("_SpringTint");
                break;
            case Season.Summer:
                baseColor = m_seasonVegetationMaterial.GetColor("_SummerTint");
                break;
            case Season.Winter:
                baseColor = m_seasonVegetationMaterial.GetColor("_WinterTint");
                break;
        }
    
        return baseColor;
    }

    public DateTime GetDateTimeYearData()
    {
        return new DateTime(m_year, (int)m_month, m_monthDay, m_hour, m_minute, 0);
    }

    public DayPeriod GetDayPeriod()
    {
        return m_dayPeriod;
    }

    public Season GetCurrentSeason()
    {
        return m_currentSeason;
    }


    public void RegisterShadow(ShadowInstance shadow)
    {
        m_shadows.Add(shadow);
    }

    public void UnregisterShadow(ShadowInstance shadow)
    {
        m_shadows.Remove(shadow);
    }

    public  void RegisterLightBlender(LightInterpolator interpolator)
    {
        m_lightBlender.Add(interpolator);
    }

    public void UnregisterLightBlender(LightInterpolator interpolator)
    {
        m_lightBlender.Remove(interpolator);
    }


    public float CovertGameHoursToRealTimeInSecs(float time)
    {
        float minutesToLastADay = MinutesToLastADay;
        float secondsPerInGameHour = (minutesToLastADay / 24f) * 60f;
        return (time * secondsPerInGameHour);
    }



}

public enum Season { Spring, Summer, Autumn, Winter }
public enum DayPeriod { Night, Morning, Afternoon, Evening }
public enum Month
{
    January = 1, February, March, April, May, June,
    July, August, September, October, November, December
}

public enum WeekDay
{
    Sunday = 0, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday
}

public enum WeatherBehaviour
{
    None,
    Normal,
    Moderate,
    Heavy
}

public class RunningWeather
{
    readonly Dictionary<WeatherType, WeatherBehaviour> m_runningWeather = new();

    public void AddWeather(WeatherType type, WeatherBehaviour behaviour)
    {
   
        if (type == WeatherType.None )
            return;

        m_runningWeather[type] = behaviour; // Adds or updates

    }

    public void RemoveWeather(WeatherType type, WeatherBehaviour behaviour)
    {

        if (type == WeatherType.None)
            return;

        m_runningWeather.Remove(type);
    }


    public bool IsRunningWeatherTypeWithBehaviour(WeatherType type ,WeatherBehaviour behaviour)
    {

        if (m_runningWeather.TryGetValue(type, out WeatherBehaviour value))
        {
            // If no specific behaviour is required, just check if the type is running
            if (behaviour == WeatherBehaviour.None)
                return true;

            // Otherwise, compare behaviours
            return value == behaviour;
        }
        return false;
    }

    public Dictionary<WeatherType, WeatherBehaviour> GetRunningWeather()
    {
        return m_runningWeather;
    }
}