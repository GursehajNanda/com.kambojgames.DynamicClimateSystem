using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "ClimateData", menuName = "ScriptableObject/ClimateData")]
public class ClimateDataSO : ScriptableObject
{
    [Header("Date Time And Day Data")]
    [Min(1)]
    [SerializeField] private int m_year;
    [SerializeField] private Month m_month = Month.January;
    [Range(1, 31)]
    [SerializeField] private int m_monthDay;
    [Range(0, 23)]
    [SerializeField] private int m_hour;
    [Range(0, 59)]
    [SerializeField] private int m_minute;


    [Header("Season Data")]
    [SerializeField]  string m_currentSeason;


    private static ClimateDataSO m_instance;

    public static ClimateDataSO Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = Resources.Load<ClimateDataSO>("ClimateData");
                if(m_instance == null)
                {
                    Debug.LogError("ClimateDataSO instance not found in Resources.");
                }
                
            }
            return m_instance;
        }
    }

    public void SetYear(int Year)
    {
        m_year = Year;
    }

    public void SetMonth(Month month)
    {
        m_month = month;
    }

    public void SetMonthDay(int monthDay)
    {
        m_monthDay = monthDay;
    }

    public void SetHourOfDay(int hour)
    {
        m_hour = hour;
    }

    public void SetMinuteOfHour(int minute)
    {
        m_minute = minute;
    }

    public void  SetDateTimeYearData(DateTime dateTimeYearData)
    {
        m_year = dateTimeYearData.Year;
        m_month = (Month)(dateTimeYearData.Month);
        m_monthDay = dateTimeYearData.Day;
        m_hour = dateTimeYearData.Hour;
        m_minute = dateTimeYearData.Minute;
    }

    public DateTime GetDateTimeYearData()
    {
        DateTime dateTimeYearData = new DateTime(m_year, (int)m_month, m_monthDay, m_hour, m_minute, 0);
        return dateTimeYearData;
    }
}


