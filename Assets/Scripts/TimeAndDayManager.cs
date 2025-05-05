using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeAndDayManager
{
    private float m_minutesToLastADay;
    private DateTime m_currentDateTime;
    private Month m_currentMonth;

    private int m_currentYear;
    private int m_currentDayOfWeek; //0 to 1 (Sun to Sat)
    private int m_lastDay = -1;
    private ClimateData m_climateData;

    public TimeAndDayManager(float minutesToLastADay)
    {

        DateTime currentDateTime = ClimateData.Instance.GetDateTimeYearData();

        m_minutesToLastADay = minutesToLastADay;
        m_currentDateTime = currentDateTime;
        m_currentMonth = (Month)currentDateTime.Month;
        m_currentYear = currentDateTime.Year;
        m_currentDayOfWeek = (int)currentDateTime.DayOfWeek;

        m_climateData = ClimateData.Instance;
    }


    public void UpdateDateAndTime()
    {
        float inGameSecondsPerRealSecond = 86400f / (m_minutesToLastADay * 60.0f);
        m_currentDateTime = m_currentDateTime.AddSeconds(Time.deltaTime * inGameSecondsPerRealSecond);

        // Check if a new day has started
        if (m_currentDateTime.DayOfYear != m_lastDay)
        {
            m_lastDay = m_currentDateTime.DayOfYear;

            m_currentDayOfWeek = (m_currentDayOfWeek + 1) % 7;

            if (m_currentDateTime.Year != m_currentYear)
            {
                m_currentYear = m_currentDateTime.Year;
                m_climateData.SetYear(m_currentYear);
            }

            UpdateMonth();
        }

        m_climateData.SetHourOfDay(m_currentDateTime.Hour);
        m_climateData.SetMinuteOfHour(m_currentDateTime.Minute);
    }


    private void UpdateMonth()
    {
        // Update month from DateTime
        m_currentMonth = (Month)m_currentDateTime.Month;
        m_climateData.SetMonth(m_currentMonth);

        // Update day of month
        m_climateData.SetMonthDay(m_currentDateTime.Day);

        //Set Seasons
        int currentMonth = m_currentDateTime.Month;

        if (currentMonth >= 3 && currentMonth < 6)
        {
            m_climateData.SetCurrentSeason(Season.Spring);
        }
        else if(currentMonth >= 6 && currentMonth < 9)
        {
            m_climateData.SetCurrentSeason(Season.Summer);
        }
        else if(currentMonth >= 9 && currentMonth < 12)
        {
            m_climateData.SetCurrentSeason(Season.Autumn);
        }
        else
        {
            m_climateData.SetCurrentSeason(Season.Winter);
        }
    }

}

