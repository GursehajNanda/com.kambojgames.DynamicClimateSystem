using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;


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
    [SerializeField][Min(1f)] private float m_minutesToLastADay;


    [Header("Seasonal Day Light")]
    [SerializeField] private Gradient m_summerColorGradient;
    [SerializeField] private Gradient m_winterColorGradient;
    [SerializeField] private Gradient m_springColorGradient;
    [SerializeField] private Gradient m_autumnColorGradient;

    [Header("Weather")]
    [SerializeField] List<Weather> m_weatherObjects;
    [SerializeField] float m_autoWeatherActiveGameTime = 1.0f;

    private float m_cloudsStrength;
    private DayPeriod m_dayPeriod;
    private Season m_currentSeason;
    private static ClimateData m_instance;
    private Material m_seasonMaterial;
    private Material m_seasonVegetationMaterial;
    private List<ShadowInstance> m_shadows = new();
    private List<LightInterpolator> m_lightBlender = new();
    private RunningWeather m_runningWeather = new();
    private DateTime m_currentDateTime;
    private Timer m_autoWeatherCheckTimer;

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
                if (m_instance == null)
                {
                    UnityEngine.Debug.LogError("ClimateData instance not found in Resources.");
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

        Weather[] weatherObjects = Resources.LoadAll<Weather>("Weather");

        m_weatherObjects.Clear();
        foreach (Weather weatherObj in weatherObjects)
        {
            m_weatherObjects.Add(weatherObj);
            if (weatherObj.WeatherType != WeatherType.Clear)
            {
                weatherObj.SetIsColldown(true);
            }

        }

        

        m_currentDateTime = new DateTime(m_year, (int)m_month, m_monthDay, m_hour, m_minute, 0);
    }

    public void StartWeather()
    {
        foreach (Weather weather in m_weatherObjects)
        {
            weather.Initialize();
        }

        foreach (Weather weather in m_weatherObjects)
        {
            weather.DeactivateWeather();
        }

        m_runningWeather = new();
        m_cloudsStrength = 0;
        float checkRealTime = CovertGameTimeToRealTimeInSecs(m_autoWeatherActiveGameTime);
        m_autoWeatherCheckTimer = new Timer(1.0f, checkRealTime, null, UpdateWeatherConditions);
        m_autoWeatherCheckTimer.Start();

        AddRunningWeather(WeatherType.Clear, WeatherBehaviour.None);
    }

    public void UpdateWeather()
    {

        for (int i = 0; i < m_weatherObjects.Count; i++)
        {
            m_weatherObjects[i].UpdateWeather();
        }


        if (m_runningWeather.GetRunningWeather().Count == 0)
        {
            AddRunningWeather(WeatherType.Clear, WeatherBehaviour.None);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Dictionary<WeatherType, WeatherBehaviour> runningWeather = m_runningWeather.GetRunningWeather();

            foreach (KeyValuePair<WeatherType, WeatherBehaviour> kvp in runningWeather)
            {
                UnityEngine.Debug.Log("Running Weather is" + $"Key: {kvp.Key}, Value: {kvp.Value}");
            }

        }

        if(m_autoWeatherCheckTimer.IsTimerRunning())
        {
            m_autoWeatherCheckTimer.Update(Time.deltaTime);
        }
        else
        {
            m_autoWeatherCheckTimer.Reset();
            m_autoWeatherCheckTimer.Start();
        }
      

    }

    public void ActivateWeatherObject(Weather weatherObject)
    {
        if (m_weatherObjects.Contains(weatherObject))
        {
            weatherObject.ActivateWeather();
        }
    }



    public void AddRunningWeather(WeatherType type, WeatherBehaviour behaviour)
    {
        m_runningWeather.AddWeather(type, behaviour);
        UpdateWeatherConditions();
    }

    public void RemoveRunningWeather(WeatherType type, Weather weatherObject = null)
    {
        foreach (Weather weather in m_weatherObjects)
        {
            if (weather.WeatherType == type)
            {
                weather.DeactivateWeather();
            }
        }

 

        m_runningWeather.RemoveWeather(type);

        UpdateWeatherConditions();
    }

    public void ForceWeatherEffects(WeatherType type, WeatherBehaviour behaviour)
    {
        foreach (Weather weather in m_weatherObjects)
        {
            if (weather.WeatherType == type)
            {
                weather.StopCoolDown();
            }
        }
        AddRunningWeather(type, behaviour);
    }
    public void ResetWeatherEffects()
    {
        foreach (Weather weather in m_weatherObjects)
        {
            weather.ResetWeather();
        }
    }

    private void UpdateWeatherConditions()
    {
        for (int i = 0; i < m_weatherObjects.Count; i++)
        {
            m_weatherObjects[i].ActivateWeather();
        }

    }


    public bool IsRunningWeatherTypeWithBehaviour(WeatherType type, WeatherBehaviour behaviour = WeatherBehaviour.None)
    {
        return m_runningWeather.IsRunningWeatherTypeWithBehaviour(type, behaviour);
    }

    public bool IsRunningWeatherTypeWithBehaviour(WeatherTypeFlags weatherFlag, WeatherBehaviour behaviour = WeatherBehaviour.None)
    {
        bool foundWeather = false;
        foreach (WeatherType type in (WeatherType[])System.Enum.GetValues(typeof(WeatherType)))
        {
            // Convert to bitmask only if not None
            WeatherTypeFlags flag = WeatherTypeFlags.None;
            if (type != WeatherType.None)
            {
                flag = (WeatherTypeFlags)(1 << ((int)type - 1));
            }

            if (weatherFlag.HasFlag(flag))
            {
                if (m_runningWeather.IsRunningWeatherTypeWithBehaviour(type, behaviour))
                {
                    foundWeather = true;
                }
            }
        }

        return foundWeather;

    }
    
    public WeatherBehaviour GetCurrentWeatherBehaviour(WeatherType type)
    {
        if(m_runningWeather.GetRunningWeather().TryGetValue(type, out WeatherBehaviour behaviour))
        {
            return behaviour;
        }
        return WeatherBehaviour.None;
    }

    public void SetCloudStrength(float value)
    {
        if (value < 0 || value > 1)
        {
            UnityEngine.Debug.Log("Cloud strength value must be between 0 and 1");
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
        UpdateCurrentDateTime(month: (int)month);
    }

    public void SetDayInMonth(int dayInMonth)
    {
        UpdateCurrentDateTime(day: dayInMonth);
    }


    public void SetHourOfDay(int hour)
    {
        UpdateCurrentDateTime(hour: hour);
    }

    public void SetMinuteOfHour(int minute)
    {
        UpdateCurrentDateTime(minute: minute);
    }

    public void SetDayPeriod(DayPeriod dayPeriod)
    {
        m_dayPeriod = dayPeriod;
    }

    public void SetDateTimeYearData(DateTime dateTimeYearData)
    {
        m_currentDateTime = dateTimeYearData;

        m_year = dateTimeYearData.Year;
        m_month = (Month)(dateTimeYearData.Month);
        m_monthDay = dateTimeYearData.Day;
        m_hour = dateTimeYearData.Hour;
        m_minute = dateTimeYearData.Minute;
    }

    public void UpdateCurrentDateTime(
    int? year = null,
    int? month = null,
    int? day = null,
    int? hour = null,
    int? minute = null,
    int? second = null)
    {
        m_currentDateTime = new DateTime(
            year ?? m_currentDateTime.Year,
            month ?? m_currentDateTime.Month,
            day ?? m_currentDateTime.Day,
            hour ?? m_currentDateTime.Hour,
            minute ?? m_currentDateTime.Minute,
            second ?? m_currentDateTime.Second
        );
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
        return m_currentDateTime;
    }

    public float GetTimeInFraction()
    {
        return m_hour + (m_minute / 60f);
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

    public void RegisterLightBlender(LightInterpolator interpolator)
    {
        m_lightBlender.Add(interpolator);
    }

    public void UnregisterLightBlender(LightInterpolator interpolator)
    {
        m_lightBlender.Remove(interpolator);
    }


    public float CovertGameTimeToRealTimeInSecs(float time, bool isTimeInMins = false)
    {
        if (isTimeInMins)
        {
            time = time / 60;
        }

        float minutesToLastADay = MinutesToLastADay;
        float secondsPerInGameHour = (minutesToLastADay / 24f) * 60f;
        return (time * secondsPerInGameHour);
    }



    public void UpdateMinutesToLastTheDay(float time)
    {
        if (time < 0)
        {
            UnityEngine.Debug.LogError("Minutes to Last The Day cannot be negative");
        }

        m_minutesToLastADay = time;

    }


}

public enum Season { Spring, Summer, Autumn, Winter }
public enum DayPeriod { Night, Morning, Afternoon, Evening }
public enum Month
{
    January = 1, February, March, April, May, June,
    July, August, September, October, November, December
}

[System.Flags]
public enum WeekDayFlags
{
    None = 0,
    Sunday = 1 << 0,
    Monday = 1 << 1,
    Tuesday = 1 << 2,
    Wednesday = 1 << 3,
    Thursday = 1 << 4,
    Friday = 1 << 5,
    Saturday = 1 << 6,
    AllDays = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday
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

        if (type == WeatherType.None)
            return;

        m_runningWeather[type] = behaviour; // Adds or updates

    }

    public void RemoveWeather(WeatherType type)
    {

        if (type == WeatherType.None)
            return;

        m_runningWeather.Remove(type);
    }


    public bool IsRunningWeatherTypeWithBehaviour(WeatherType type, WeatherBehaviour behaviour)
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