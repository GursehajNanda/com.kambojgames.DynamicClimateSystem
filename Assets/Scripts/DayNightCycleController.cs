using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;
using KambojGames.Utilities2D;


[Serializable]
public class DayNightCycleController 
{
    [Header("Sunlight Movement")]
    [SerializeField] private Transform m_sunTransform;
    [SerializeField] private Transform m_orbitCenter;
    [SerializeField] private float m_orbitRadius = 20f;

    [Header("Lighting")]
    [SerializeField] private Light2D m_globalLight;

    [Header("Night Lights")]
    [SerializeField] private List<Light2D> m_nightLights;

    [Header("Time Period Settings")]
    [SerializeField] private float m_morningStart = 4f;
    [SerializeField] private float m_afternoonStart = 12f;
    [SerializeField] private float m_eveningStart = 17f;
    [SerializeField] private float m_nightStart = 21f;

    [Header("Blend Factor")]
    [SerializeField] private float m_seasonBlendFactor; // 0 = current season, 1 = next season

    private Season m_nextSeason;
    private AnimationCurve m_summerCurve;
    private AnimationCurve m_winterCurve;
    private AnimationCurve m_springCurve;
    private AnimationCurve m_autumnCurve;
    private AnimationCurve m_nightLightFadeCurve;

    private float m_inGameTime; // 0 - 24
    private DayPeriod m_currentPeriod;
    private Season m_currentSeason;
    private bool IsInitialzied = false;
    private KeyValuePairList<Season, AnimationCurve> m_seasonCurves;
    private KeyValuePairList<Season, Gradient> m_seasonGradients;

    public void Initialize()
    {
        m_currentSeason = ClimateDataSO.Instance.GetCurrentSeason();

        m_summerCurve = new AnimationCurve(
            new Keyframe(0f, 0.2f),  // midnight - dim
            new Keyframe(0.25f, 0.8f), // 6 AM
            new Keyframe(0.5f, 1f),   // noon - peak light
            new Keyframe(0.75f, 0.8f), // 6 PM
            new Keyframe(1f, 0.2f)   // midnight again
       );

        m_winterCurve = new AnimationCurve(
            new Keyframe(0f, 0.05f),
            new Keyframe(0.3f, 0.4f),
            new Keyframe(0.5f, 0.6f),
            new Keyframe(0.7f, 0.4f),
            new Keyframe(1f, 0.05f)
        );

        m_springCurve = new AnimationCurve(
            new Keyframe(0f, 0.1f),
            new Keyframe(0.25f, 0.6f),
            new Keyframe(0.5f, 0.9f),
            new Keyframe(0.75f, 0.6f),
            new Keyframe(1f, 0.1f)
        );

        m_autumnCurve = new AnimationCurve(
            new Keyframe(0f, 0.1f),
            new Keyframe(0.3f, 0.5f),
            new Keyframe(0.5f, 0.7f),
            new Keyframe(0.7f, 0.5f),
            new Keyframe(1f, 0.1f)
        );

        m_nightLightFadeCurve = new AnimationCurve(
            new Keyframe(0f, 0f),              // Midnight - off
            new Keyframe(0.167f, 0f),          // 4 AM - still off
            new Keyframe(0.25f, 1f),           // 6 AM - lights fade out
            new Keyframe(0.79f, 0f),           // 19 PM - lights start fading in
            new Keyframe(0.875f, 1f),          // 21 PM - fully on
            new Keyframe(1f, 1f)               // End of day - still on
        );

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
      
    }

    void UpdateSunPosition()
    {
        float angle = (m_inGameTime / 24f) * 360f;
        Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * m_orbitRadius;
        m_sunTransform.position = m_orbitCenter.position + offset;
        m_sunTransform.right = (m_orbitCenter.position - m_sunTransform.position).normalized;
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
            light.intensity = fadeFactor;
        }
    }

    void UpdateDayPeriod()
    {
        if (m_inGameTime >= m_nightStart || m_inGameTime < m_morningStart)
            m_currentPeriod = DayPeriod.Night;
        else if (m_inGameTime >= m_morningStart && m_inGameTime < m_afternoonStart)
            m_currentPeriod = DayPeriod.Morning;
        else if (m_inGameTime >= m_afternoonStart && m_inGameTime < m_eveningStart)
            m_currentPeriod = DayPeriod.Afternoon;
        else
            m_currentPeriod = DayPeriod.Evening;

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

}
