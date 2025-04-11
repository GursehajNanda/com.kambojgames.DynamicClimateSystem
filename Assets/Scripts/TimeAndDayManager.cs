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


    public TimeAndDayManager(float minutesToLastADay)
    {

        DateTime currentDateTime = ClimateDataSO.Instance.GetDateTimeYearData();

        m_minutesToLastADay = minutesToLastADay;
        m_currentDateTime = currentDateTime;
        m_currentMonth = (Month)currentDateTime.Month;
        m_currentYear = currentDateTime.Year;
        m_currentDayOfWeek = (int)currentDateTime.DayOfWeek;
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
                ClimateDataSO.Instance.SetYear(m_currentYear);
            }

            UpdateMonth();
        }

        ClimateDataSO.Instance.SetHourOfDay(m_currentDateTime.Hour);
        ClimateDataSO.Instance.SetMinuteOfHour(m_currentDateTime.Minute);
    }


    private void UpdateMonth()
    {
        // Update month from DateTime
        m_currentMonth = (Month)m_currentDateTime.Month;
        ClimateDataSO.Instance.SetMonth(m_currentMonth);

        // Update day of month
        ClimateDataSO.Instance.SetMonthDay(m_currentDateTime.Day);

        //Set Seasons
        if(m_currentDateTime.Month >= 3 && m_currentDateTime.Month < 6)
        {
            ClimateDataSO.Instance.SetCurrentSeason(Season.Spring);
        }
        else if(m_currentDateTime.Month >=6 && m_currentDateTime.Month<9)
        {
            ClimateDataSO.Instance.SetCurrentSeason(Season.Summer);
        }
        else if(m_currentDateTime.Month >= 9 && m_currentDateTime.Month <12)
        {
            ClimateDataSO.Instance.SetCurrentSeason(Season.Autumn);
        }
        else
        {
            ClimateDataSO.Instance.SetCurrentSeason(Season.Winter);
        }
    }

}

