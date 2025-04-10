using UnityEngine;
using System;

public class ClimateManager : MonoBehaviour
{
    [Min(1.0f)]
    [SerializeField] private float m_minutesToLastADay;
    [SerializeField] private ClimateDataSO m_climateDataSO;

    TimeAndDayManager m_timeAndDayManager;

    private void Start()
    {
        m_timeAndDayManager = new TimeAndDayManager(m_minutesToLastADay, m_climateDataSO);
    }

    private void Update()
    {
        m_timeAndDayManager.UpdateDateAndTime();

        if(Input.GetKeyDown(KeyCode.Space))
        {
            //Get it through ClimateDataSO
            Debug.Log("Current Year: " + m_climateDataSO.GetDateTimeYearData().Year);
            Debug.Log("Current Month: " + (Month)m_climateDataSO.GetDateTimeYearData().Month); ;
            Debug.Log("Current Month Day: " + m_climateDataSO.GetDateTimeYearData().Day);
            Debug.Log("Current Week Day: " +(WeekDay)m_climateDataSO.GetDateTimeYearData().DayOfWeek); ;
            Debug.Log("Current Time Hour: " + m_climateDataSO.GetDateTimeYearData().Hour); ;
            Debug.Log("Current Time Minute: " + m_climateDataSO.GetDateTimeYearData().Minute); ;
        }
    }
}
