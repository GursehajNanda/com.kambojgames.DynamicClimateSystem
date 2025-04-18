using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using KambojGames.Utilities2D;


[Serializable]
public class SeasonCycleController
{
    private Light2D m_sun;
    Light2D m_globalLight;
    private KeyValuePairList<Season, AnimationCurve> m_seasonCurves;
    private KeyValuePairList<Season, Gradient> m_seasonGradients;
    private float m_inGameTime;
    private Season m_currentSeason;
    private Season m_nextSeason;
    private float m_seasonBlendFactor = 0.5f;
    private int m_currentDay;
    private bool m_isInitialzied = false;
    
    private AnimationCurve m_summerCurve = new AnimationCurve(
        new Keyframe(0f, 0.0f),  // midnight - dim
        new Keyframe(0.25f, 0.8f), // 6 AM
        new Keyframe(0.5f, 1f),   // noon - peak light
        new Keyframe(0.75f, 0.8f), // 6 PM
        new Keyframe(1f, 0.0f)   // midnight again
   );

    //Make two curves 1 for ambient and 1 for global
    private AnimationCurve m_winterCurve = new AnimationCurve(
            new Keyframe(0f, 0.0f),
            new Keyframe(0.3f, 0.4f),
            new Keyframe(0.5f, 0.7f),
            new Keyframe(0.7f, 0.4f),
            new Keyframe(1f, 0.0f)
        );

    private AnimationCurve m_springCurve = new AnimationCurve(
            new Keyframe(0f, 0.0f),
            new Keyframe(0.25f, 0.6f),
            new Keyframe(0.5f, 0.9f),
            new Keyframe(0.75f, 0.6f),
            new Keyframe(1f, 0.0f)
        );
    private AnimationCurve m_autumnCurve = new AnimationCurve(
            new Keyframe(0f, 0.0f),
            new Keyframe(0.3f, 0.5f),
            new Keyframe(0.5f, 0.8f),
            new Keyframe(0.7f, 0.5f),
            new Keyframe(1f, 0.0f)
        );


    private Dictionary<Season, int> m_seasonStartDayOfYear = new Dictionary<Season, int>
        {
            { Season.Spring, 60 },   // March 1
            { Season.Summer, 151 },  // June 1
            { Season.Autumn, 243 },  // Sept 1
            { Season.Winter, 334 }   // Dec 1
        };

    private Dictionary<Season, int> m_seasonPeakDayOfYear = new Dictionary<Season, int>
        {
            { Season.Spring, 105 },   // April 15
            { Season.Summer, 196 },   // July 15
            { Season.Autumn, 288 },   // Oct 15
            { Season.Winter, 15 }     // Jan 15
        };

    public void Initialize(Light2D sun,Light2D globalLight)
    {
        m_sun = sun;
        m_globalLight = globalLight;
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


        m_currentDay = ClimateDataSO.Instance.GetDateTimeYearData().Day;
        UpdateSeasonalCurves();

        m_isInitialzied = true;
    }


    public void UpdateSeasons()
    {
        if (!m_isInitialzied) { Debug.LogError("SeasonCycleController is not Initialzed, Please Initialize it correctly"); }

        DateTime currentTime = ClimateDataSO.Instance.GetDateTimeYearData();

        // Convert hour + minute into float (e.g., 14.5f for 2:30 PM)
        m_inGameTime = currentTime.Hour + (currentTime.Minute / 60f);

        UpdateSeasonalCurves();
        UpdateSeasonalLighting();
        UpdateSeasonalBlend();
       

    }

    void UpdateSeasonalLighting()
    {
        float t = m_inGameTime / 24f;


        AnimationCurve currentCurve = m_seasonCurves.GetValueByKey(m_currentSeason);
        AnimationCurve nextCurve = m_seasonCurves.GetValueByKey(m_nextSeason);

        float currentValue = currentCurve.Evaluate(t);
        float nextValue = nextCurve.Evaluate(t);

        float blended = Mathf.Lerp(currentValue, nextValue, m_seasonBlendFactor);
        m_sun.intensity = blended;

        Gradient currentGradient = m_seasonGradients.GetValueByKey(m_currentSeason);
        Gradient nextGradient = m_seasonGradients.GetValueByKey(m_nextSeason);

        Color currentColor = currentGradient.Evaluate(t);
        Color nextColor = nextGradient.Evaluate(t);

        m_globalLight.color = Color.Lerp(currentColor, nextColor, m_seasonBlendFactor);
    }


    void UpdateSeasonalCurves()
    {
        Season currentSeasonFromData = ClimateDataSO.Instance.GetCurrentSeason();
        if (m_currentSeason == currentSeasonFromData) return;

        m_currentSeason = currentSeasonFromData;
        int nextSeasonValue = ((int)m_currentSeason + 1) % 4;
        m_nextSeason = (Season)nextSeasonValue;
    }

    //Can be disabled And set the m_seasonBlendFactor value manually
    void UpdateSeasonalBlend()
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

        //D0 Vegation check here after new day check
        UpdateSeasonalVegetationBlend(daysInYear); //Update Vegetaion Blend


    }

    void UpdateSeasonalVegetationBlend(int daysInYear)
    {
        Season current = m_currentSeason;
        Season next = m_nextSeason;
        int currentPeak = 0, nextPeak = 0;

        foreach (Season season in Enum.GetValues(typeof(Season)))
        {
            Season nextSeason = m_nextSeason;
            int peak1 = m_seasonPeakDayOfYear[season];
            int peak2 = m_seasonPeakDayOfYear[nextSeason];

            if (peak2 < peak1) peak2 += daysInYear;

            int currentDay = m_currentDay < peak1 ? m_currentDay + daysInYear : m_currentDay;

            if (currentDay >= peak1 && currentDay < peak2)
            {
                current = season;
                next = nextSeason;
                currentPeak = peak1;
                nextPeak = peak2;
                break;
            }
        }

        float t = (float)(m_currentDay + (m_currentDay < currentPeak ? daysInYear : 0) - currentPeak) / (nextPeak - currentPeak);

        float startBlend = GetBaseBlend(current);
        float endBlend = GetBaseBlend(next);

        if (endBlend < startBlend)
            endBlend += 1.0f;

        float blendValue = Mathf.Lerp(startBlend, endBlend, t) % 1.0f;

        ClimateDataSO.Instance.SetMaterialSesaonalBlend(blendValue);

    }





    private float GetBaseBlend(Season season)
    {
        return season switch
        {
            Season.Spring => 0f,
            Season.Summer => 0.25f,
            Season.Autumn => 0.5f,
            Season.Winter => 0.75f,
            _ => 0f
        };
    }


}
