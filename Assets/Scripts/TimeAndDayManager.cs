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
    private int m_currentYearDay; // 1 to 365


    ClimateDataSO m_climateData;

    public TimeAndDayManager(float minutesToLastADay,ClimateDataSO climateData)
    {
        m_climateData = climateData;

        DateTime currentDateTime = climateData.GetDateTimeYearData();

        m_minutesToLastADay = minutesToLastADay;
        m_currentDateTime = currentDateTime;
        m_currentMonth = (Month)currentDateTime.Month;
        m_currentYear = currentDateTime.Year;
        m_currentYearDay = currentDateTime.DayOfYear;
        m_currentDayOfWeek = (int)currentDateTime.DayOfWeek;
    }


    public void UpdateDateAndTime()
    {
      
        float inGameSecondsPerRealSecond = 86400f / (m_minutesToLastADay*60.0f); //60 seconds × 60 minutes × 24 hours = 86,400 seconds

        m_currentDateTime = m_currentDateTime.AddSeconds(Time.deltaTime * inGameSecondsPerRealSecond);

        if (m_currentDateTime.TimeOfDay.TotalSeconds >= 86400) // a day has passed
        {
            m_currentDayOfWeek = (m_currentDayOfWeek + 1) % 7;
            m_currentYearDay++;

            if (m_currentYearDay > 365)
            {
                m_currentYearDay = 1;
                m_currentYear++;
                m_climateData.SetYear(m_currentYear);
            }

            UpdateMonth();
        }
        m_climateData.SetHourOfDay(m_currentDateTime.Hour);
        m_climateData.SetMonthDay(m_currentDateTime.Minute);
    }


    private void UpdateMonth()
    {
        int currentMonthInt = (int)m_currentMonth;
        int daysThisMonth = DateTime.DaysInMonth(m_currentYear, currentMonthInt);
        int dayOfMonth = m_currentDateTime.Day;

        if (dayOfMonth > daysThisMonth)
        {
            /*This moves the current date backward by the number of days in the month.
          For example, if January has 31 days and you’re at Feb 1 (after going over 31), it subtracts 31 days to go back to Jan 1.
          This sets the base before you switch to the new month.*/
            m_currentDateTime = m_currentDateTime.AddDays(-daysThisMonth);
            m_currentMonth = (Month)(((int)m_currentMonth % 12) + 1);
            m_climateData.SetMonth(m_currentMonth);
        }

        m_climateData.SetMonthDay(daysThisMonth);
    }

}

public enum Month
{
    January = 1, February, March, April, May, June,
    July, August, September, October, November, December
}

public enum WeekDay
{
    Sunday = 0, Monday,Tuesday,Wednesday,Thursday,Friday,Saturday
}