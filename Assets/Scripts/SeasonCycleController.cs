using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using KambojGames.Utilities2D;


[Serializable]
public class SeasonCycleController
{
    private Light2D m_sun;
    private Light2D m_moon;
    private KeyValuePairList<Season, AnimationCurve> m_seasonDayCurves;
    private KeyValuePairList<Season, AnimationCurve> m_seasonNightCurves;
    private KeyValuePairList<Season, Gradient> m_seasonGradients;
    private float m_inGameTime;
    private Season m_currentSeason;
    private Season m_nextSeason;
    private float m_seasonBlendFactor = 0.5f;
    private int m_currentDay;
    private bool m_isInitialzied = false;

    private AnimationCurve m_summerSunCurve = new AnimationCurve(
        new Keyframe(0f, 0.0f),  // midnight - dim
        new Keyframe(0.1875f, 0.03f), // pre dawn 04:30 
        new Keyframe(0.25f, 0.7f), // 6 AM
        new Keyframe(0.5f, 0.9f),   // noon - peak light
        new Keyframe(0.75f, 0.7f), // 6 PM
        new Keyframe(0.89f, 0.0f)   // Around 9:36 night again
   );

    private AnimationCurve m_springSunCurve = new AnimationCurve(
        new Keyframe(0f, 0.0f),
        new Keyframe(0.20f, 0.025f),
        new Keyframe(0.25f, 0.6f),
        new Keyframe(0.5f, 0.8f),
        new Keyframe(0.75f, 0.6f),
        new Keyframe(0.85f, 0.0f)
   );


    private AnimationCurve m_winterSunCurve = new AnimationCurve(
        new Keyframe(0f, 0.0f),
        new Keyframe(0.2f, 0.0f),
        new Keyframe(0.3f, 0.4f),
        new Keyframe(0.5f, 0.65f),
        new Keyframe(0.68f, 0.4f),
        new Keyframe(0.76f, 0.0f)
   );

    private AnimationCurve m_autumnSunCurve = new AnimationCurve(
        new Keyframe(0f, 0.0f),
        new Keyframe(0.20f, 0.01f),
        new Keyframe(0.3f, 0.5f),
        new Keyframe(0.5f, 0.75f),
        new Keyframe(0.7f, 0.5f),
        new Keyframe(0.80f, 0.0f)
   );




    private AnimationCurve m_summerMoonCurve = new AnimationCurve(
        new Keyframe(0f, 0.2f),       // Midnight - already visible
        new Keyframe(0.1875f, 0.05f), // Just before sunrise
        new Keyframe(0.25f, 0.0f),    // Sunrise
        new Keyframe(0.89f, 0.0f),    // Sunset starts
        new Keyframe(0.95f, 0.1f),    // Moon is rising
        new Keyframe(1f, 0.2f)        // Midnight again - peak moonlight
   );

    private AnimationCurve m_autumnMoonCurve = new AnimationCurve(
        new Keyframe(0f, 0.25f),
        new Keyframe(0.2f, 0.05f),
        new Keyframe(0.3f, 0.0f),
        new Keyframe(0.80f, 0.0f),    // Sun has set
        new Keyframe(0.9f, 0.15f),
        new Keyframe(1f, 0.25f)
    );

    private AnimationCurve m_winterMoonCurve = new AnimationCurve(
        new Keyframe(0f, 0.3f),
        new Keyframe(0.2f, 0.05f),
        new Keyframe(0.3f, 0.0f),
        new Keyframe(0.76f, 0.0f),    // Sun goes down
        new Keyframe(0.85f, 0.1f),
        new Keyframe(1f, 0.3f)
    );

    private AnimationCurve m_springMoonCurve = new AnimationCurve(
      new Keyframe(0f, 0.22f),
      new Keyframe(0.20f, 0.05f),
      new Keyframe(0.25f, 0.0f),
      new Keyframe(0.85f, 0.0f),    // After sunset
      new Keyframe(0.92f, 0.18f),
      new Keyframe(1f, 0.22f)
  );




    private readonly Dictionary<Season, Vector2> m_sunActiveTimeRanges = new Dictionary<Season, Vector2>
    {
        { Season.Summer, new Vector2(0.1875f, 0.89f) },
        { Season.Autumn, new Vector2(0.20f, 0.80f) },
        { Season.Winter, new Vector2(0.25f, 0.75f) }, // example range for winter
        { Season.Spring, new Vector2(0.20f, 0.85f) }
    };

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

    public void Initialize(Light2D sun, Light2D moon)
    {
        m_sun = sun;
        m_moon = moon;
        m_seasonDayCurves = new KeyValuePairList<Season, AnimationCurve>();
        m_seasonDayCurves.Add(Season.Spring, m_springSunCurve);
        m_seasonDayCurves.Add(Season.Summer, m_summerSunCurve);
        m_seasonDayCurves.Add(Season.Autumn, m_autumnSunCurve);
        m_seasonDayCurves.Add(Season.Winter, m_winterSunCurve);

        m_seasonNightCurves =  new KeyValuePairList<Season, AnimationCurve>();
        m_seasonNightCurves.Add(Season.Spring, m_springMoonCurve);
        m_seasonNightCurves.Add(Season.Summer, m_summerMoonCurve);
        m_seasonNightCurves.Add(Season.Autumn, m_autumnMoonCurve);
        m_seasonNightCurves.Add(Season.Winter, m_winterMoonCurve);


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
        float t = m_inGameTime / 24f;

        UpdateSeasonalCurves();
        UpdateSeasonalAmbientLightning(t);
        UpdateSunSeasonalLighting(t);
        UpdateMoonSeasonalLighting(t);
        UpdateSeasonalBlend();
       

    }

    void UpdateSeasonalAmbientLightning(float ratio)
    {
        Gradient currentGradient = m_seasonGradients.GetValueByKey(m_currentSeason);
        Gradient nextGradient = m_seasonGradients.GetValueByKey(m_nextSeason);

        Color currentColor = currentGradient.Evaluate(ratio);
        Color nextColor = nextGradient.Evaluate(ratio);

        if (m_sunActiveTimeRanges.TryGetValue(m_currentSeason, out var range))
        {
            bool isSunTime = ratio > range.x && ratio <= range.y;

            if (isSunTime)
                m_sun.color = Color.Lerp(currentColor, nextColor, m_seasonBlendFactor);
            else
                m_moon.color = Color.Lerp(currentColor, nextColor, m_seasonBlendFactor);
        }

    }

    void UpdateSunSeasonalLighting(float ratio)
    {
      
        AnimationCurve currentCurve = m_seasonDayCurves.GetValueByKey(m_currentSeason);
        AnimationCurve nextCurve = m_seasonDayCurves.GetValueByKey(m_nextSeason);

        float currentValue = currentCurve.Evaluate(ratio);
        float nextValue = nextCurve.Evaluate(ratio);

        float blended = Mathf.Lerp(currentValue, nextValue, m_seasonBlendFactor);
        m_sun.intensity = blended;
    }

    void UpdateMoonSeasonalLighting(float ratio)
    {
        AnimationCurve currentCurve = m_seasonNightCurves.GetValueByKey(m_currentSeason);
        AnimationCurve nextCurve = m_seasonNightCurves.GetValueByKey(m_nextSeason);

        float currentValue = currentCurve.Evaluate(ratio);
        float nextValue = nextCurve.Evaluate(ratio);

        float blended = Mathf.Lerp(currentValue, nextValue, m_seasonBlendFactor);
        m_moon.intensity = blended;
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
