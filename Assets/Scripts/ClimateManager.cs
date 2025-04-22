using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ClimateManager : MonoBehaviour
{
    [Min(1.0f)]
    [SerializeField] private float m_minutesToLastADay;
    [SerializeField] private Light2D m_globalLight;
    [SerializeField] private Light2D m_sun;
    [SerializeField] private Light2D m_moon;
    [SerializeField] private ClimateDataSO m_climateDataSO;
    [SerializeField] DayNightCycleController m_dayAndNightController = new();
    [SerializeField] SeasonCycleController m_seasonCycleController = new();
    [SerializeField] CloudController m_cloudController = new();



    TimeAndDayManager m_timeAndDayManager;

    private void Start()
    {
        m_timeAndDayManager = new TimeAndDayManager(m_minutesToLastADay);
        m_dayAndNightController.Initialize(m_sun, m_moon,m_globalLight);
        m_seasonCycleController.Initialize(m_sun, m_moon);
        m_cloudController.Initialize();
    }

    private void Update()
    {
        m_timeAndDayManager.UpdateDateAndTime();
        m_dayAndNightController.UpdateNightDayCycle();
        m_seasonCycleController.UpdateSeasons();
        m_cloudController.UpdateClouds();
    }
}
