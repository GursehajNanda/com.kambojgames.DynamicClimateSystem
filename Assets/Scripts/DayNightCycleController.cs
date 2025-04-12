using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;
using KambojGames.Utilities2D;


[Serializable]
public class DayNightCycleController 
{
    [Header("Sunlight")]
    [SerializeField] private Light2D m_sun;
    [SerializeField] private Transform m_orbitCenter;
    [SerializeField] private float m_orbitRadius = 20f;
    [SerializeField] float m_intensitySmoothSpeed = 2f;

    [Header("Lighting")]
    [SerializeField] private Light2D m_globalLight;

    [Header("Night Lights")]
    [SerializeField] private List<Light2D> m_nightLights;

    [Header("Time Period Settings")]
    [SerializeField] private float m_morningStart = 4f;
    [SerializeField] private float m_afternoonStart = 12f;
    [SerializeField] private float m_eveningStart = 17f;
    [SerializeField] private float m_nightStart = 21f;



    private AnimationCurve m_summerCurve = new AnimationCurve(
            new Keyframe(0f, 0.2f),  // midnight - dim
            new Keyframe(0.25f, 0.8f), // 6 AM
            new Keyframe(0.5f, 1f),   // noon - peak light
            new Keyframe(0.75f, 0.8f), // 6 PM
            new Keyframe(1f, 0.2f)   // midnight again
       );

    private AnimationCurve m_winterCurve = new AnimationCurve(
            new Keyframe(0f, 0.05f),
            new Keyframe(0.3f, 0.4f),
            new Keyframe(0.5f, 0.6f),
            new Keyframe(0.7f, 0.4f),
            new Keyframe(1f, 0.05f)
        );

    private AnimationCurve m_springCurve = new AnimationCurve(
            new Keyframe(0f, 0.1f),
            new Keyframe(0.25f, 0.6f),
            new Keyframe(0.5f, 0.9f),
            new Keyframe(0.75f, 0.6f),
            new Keyframe(1f, 0.1f)
        );
    private AnimationCurve m_autumnCurve = new AnimationCurve(
            new Keyframe(0f, 0.1f),
            new Keyframe(0.3f, 0.5f),
            new Keyframe(0.5f, 0.7f),
            new Keyframe(0.7f, 0.5f),
            new Keyframe(1f, 0.1f)
        );


    private AnimationCurve m_nightLightFadeCurve = new AnimationCurve(
        new Keyframe(0f, 1f),           // Midnight - ON
        new Keyframe(0.167f, 0.7f),     // 04:00 — fading down
        new Keyframe(0.25f, 0f),        // 6 AM - lights fade OUT
        new Keyframe(0.75f, 0.4f),        // 6PM  — early evening (starting to rise) 
        new Keyframe(0.79f, 0.7f),        // ~7 PM  — getting brighter
        new Keyframe(1f, 1f)            // Midnight - ON
    );

    private Dictionary<Season, int> m_seasonStartDayOfYear = new Dictionary<Season, int>
        {
            { Season.Spring, 60 },   // March 1
            { Season.Summer, 151 },  // June 1
            { Season.Autumn,   243 },  // Sept 1
            { Season.Winter, 334 }   // Dec 1
        };

    private KeyValuePairList<Season, AnimationCurve> m_seasonCurves;
    private KeyValuePairList<Season, Gradient> m_seasonGradients;
    private Dictionary<Light2D, float> m_lightBaseIntensities = new Dictionary<Light2D, float>();

    private Season m_nextSeason;
    private DayPeriod m_currentPeriod;
    private Season m_currentSeason;
    private float m_seasonBlendFactor = 0.5f;
    float m_sunTargetIntensity = 1f;
    private float m_inGameTime; // 0 - 24
    private int m_currentDay;

    private bool IsInitialzied = false;


    public void Initialize()
    {
       
        m_seasonCurves = new KeyValuePairList<Season, AnimationCurve>();
        m_seasonCurves.Add(Season.Spring, m_springCurve);
        m_seasonCurves.Add(Season.Summer, m_summerCurve);
        m_seasonCurves.Add(Season.Autumn, m_autumnCurve);
        m_seasonCurves.Add(Season.Winter, m_winterCurve);

        m_seasonGradients = new KeyValuePairList<Season, Gradient>();
        m_seasonGradients.Add(Season.Spring, ClimateDataSO.Instance.SpringColorGradient);
        m_seasonGradients.Add(Season.Summer, ClimateDataSO.Instance.SummerColorGradient);
        m_seasonGradients.Add(Season.Autumn, ClimateDataSO.Instance.AutumnColorGradient);
        m_seasonGradients.Add(Season.Winter, ClimateDataSO.Instance.WinterColorGradient);

        foreach (var light in m_nightLights)
        {
            if (!m_lightBaseIntensities.ContainsKey(light))
            {
                m_lightBaseIntensities[light] = light.intensity;
            }
        }

        m_currentDay = ClimateDataSO.Instance.GetDateTimeYearData().Day;
        UpdateSeasonalCurves();

        IsInitialzied = true;
    }


    public void UpdateNightDayCycle()
    {
        if (!IsInitialzied) { Debug.LogError("DayNightCycleController is not Initialzed, Please Initialize it correctly"); }

        DateTime currentTime = ClimateDataSO.Instance.GetDateTimeYearData();

        // Convert hour + minute into float (e.g., 14.5f for 2:30 PM)
        m_inGameTime = currentTime.Hour + (currentTime.Minute / 60f);

        UpdateSunPosition();
        UpdateSeasonalCurves();
        UpdateLighting();
        UpdateDayPeriod();
        UpdateNightLights();
        UpdateSunIntensity();
        UpdateSeasonalBlendFactor();
    }

    void UpdateSunPosition()
    {
        float angle = ((m_inGameTime / 24f) * 360f) - 90f; // Shift so noon is at top (90° up)
        Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * m_orbitRadius;
        m_sun.transform.position = m_orbitCenter.position + offset;
        m_sun.transform.right = (m_orbitCenter.position - m_sun.transform.position).normalized;
    }

    void UpdateLighting()
    {
        float t = m_inGameTime / 24f;

       
        AnimationCurve currentCurve = m_seasonCurves.GetValueByKey(m_currentSeason); 
        AnimationCurve nextCurve = m_seasonCurves.GetValueByKey(m_nextSeason);

        float currentValue = currentCurve.Evaluate(t);
        float nextValue = nextCurve.Evaluate(t);

        float blended = Mathf.Lerp(currentValue, nextValue, m_seasonBlendFactor);
        m_globalLight.intensity = blended;

        Gradient currentGradient = m_seasonGradients.GetValueByKey(m_currentSeason);
        Gradient nextGradient = m_seasonGradients.GetValueByKey(m_nextSeason);

        Color currentColor = currentGradient.Evaluate(t);
        Color nextColor = nextGradient.Evaluate(t);

        m_globalLight.color = Color.Lerp(currentColor, nextColor, m_seasonBlendFactor);

    }

    void UpdateNightLights()
    {
        float t = m_inGameTime / 24f;
        float fadeFactor = Mathf.Clamp01(m_nightLightFadeCurve.Evaluate(t));

        foreach (var light in m_nightLights)
        {
            if (m_lightBaseIntensities.TryGetValue(light, out float baseIntensity))
            {
                light.intensity = baseIntensity * fadeFactor;
            }
        }
    }

    void UpdateDayPeriod()
    {
        if (m_inGameTime >= m_nightStart || m_inGameTime < m_morningStart)
        {
            m_currentPeriod = DayPeriod.Night;
        }
        else if (m_inGameTime >= m_morningStart && m_inGameTime < m_afternoonStart)
        {
            m_currentPeriod = DayPeriod.Morning;
        }
        else if (m_inGameTime >= m_afternoonStart && m_inGameTime < m_eveningStart)
        {
            m_currentPeriod = DayPeriod.Afternoon;
        }
        else
        {
            m_currentPeriod = DayPeriod.Evening;
        }

        ClimateDataSO.Instance.SetDayPeriod(m_currentPeriod);

    }

    void UpdateSeasonalCurves()
    {
        Season currentSeasonFromData = ClimateDataSO.Instance.GetCurrentSeason();
        if (m_currentSeason == currentSeasonFromData) return;

        m_currentSeason = currentSeasonFromData;
        int nextSeasonValue = ((int)m_currentSeason + 1) % 4;
        m_nextSeason = (Season)nextSeasonValue;
    }

    void UpdateSunIntensity()
    {
        if (m_currentPeriod == DayPeriod.Night)
        {
            m_sunTargetIntensity = 0.0f;
        }
        else
        {
            m_sunTargetIntensity = m_globalLight.intensity;
        }

        m_sun.intensity = Mathf.Lerp(m_sun.intensity, m_sunTargetIntensity, Time.deltaTime * m_intensitySmoothSpeed);
    }

    //Can be disabled And set the m_seasonBlendFactor value manually
    void UpdateSeasonalBlendFactor()
    {
        int newcurrentDay = ClimateDataSO.Instance.GetDateTimeYearData().DayOfYear;
        if (m_currentDay == newcurrentDay) return;

        m_currentDay = newcurrentDay;

        int year = ClimateDataSO.Instance.GetDateTimeYearData().Year;
        int daysInYear = DateTime.IsLeapYear(year) ? 366 : 365;

        int currentSeasonStart = m_seasonStartDayOfYear[m_currentSeason];
        int nextSeasonStart = m_seasonStartDayOfYear[m_nextSeason];

        // Handle wrap-around from Winter → Spring
        if (nextSeasonStart <= currentSeasonStart)
            nextSeasonStart += daysInYear;

        float seasonLength = nextSeasonStart - currentSeasonStart;
        float blendWindow = 30f;
       
        // Days into the current season
        float daysIntoSeason = m_currentDay - currentSeasonStart;
        if (daysIntoSeason < 0)
            daysIntoSeason += daysInYear;

        // Calculate blend progress in final blendWindow days
        if (daysIntoSeason >= seasonLength - blendWindow)
        {
            float blendStart = seasonLength - blendWindow;
            float blendProgress = (daysIntoSeason - blendStart) / blendWindow;
            m_seasonBlendFactor = Mathf.Clamp01(blendProgress);
        }
        else
        {
            m_seasonBlendFactor = 0f;
        }

    }
    
   
}
