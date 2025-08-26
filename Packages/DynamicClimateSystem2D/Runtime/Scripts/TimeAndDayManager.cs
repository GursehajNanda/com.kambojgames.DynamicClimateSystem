using UnityEngine;
using System;

public class TimeAndDayManager
{
    private int m_lastDay = -1;
    private ClimateData m_climateData;

    public TimeAndDayManager()
    {
        m_climateData = ClimateData.Instance;
    }


    public void UpdateDateAndTime()
    {
        float m_minutesToLastADay = m_climateData.MinutesToLastADay;
        float inGameSecondsPerRealSecond = 86400f / (m_minutesToLastADay * 60.0f);

        DateTime updated = m_climateData.GetDateTimeYearData().AddSeconds(Time.deltaTime * inGameSecondsPerRealSecond);

        // This sets: year, month, day, hour, minute
        m_climateData.SetDateTimeYearData(updated);

        if (updated.DayOfYear != m_lastDay)
        {
            m_lastDay = updated.DayOfYear;
            UpdateSeasons(updated);
        }

    }

    private void UpdateSeasons(DateTime updated)
    {
        int currentMonth = updated.Month;


        if (currentMonth >= 3 && currentMonth < 6)
        {
            m_climateData.SetCurrentSeason(Season.Spring);
        }
        else if (currentMonth >= 6 && currentMonth < 9)
        {
            m_climateData.SetCurrentSeason(Season.Summer);
        }
        else if (currentMonth >= 9 && currentMonth < 12)
        {
            m_climateData.SetCurrentSeason(Season.Autumn);
        }
        else
        {
            m_climateData.SetCurrentSeason(Season.Winter);
        }
    }



}

