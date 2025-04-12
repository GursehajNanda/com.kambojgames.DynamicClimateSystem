using UnityEngine;
using System;

public class ClimateManager : MonoBehaviour
{
    [Min(1.0f)]
    [SerializeField] private float m_minutesToLastADay;
    [SerializeField] private ClimateDataSO m_climateDataSO;
    [SerializeField] DayNightCycleController m_dayAndNightController;

    TimeAndDayManager m_timeAndDayManager;

    private void Start()
    {
        m_timeAndDayManager = new TimeAndDayManager(m_minutesToLastADay);
        m_dayAndNightController.Initialize();
    }

    private void Update()
    {
        m_timeAndDayManager.UpdateDateAndTime();
        m_dayAndNightController.UpdateNightDayCycle();
    }
}
