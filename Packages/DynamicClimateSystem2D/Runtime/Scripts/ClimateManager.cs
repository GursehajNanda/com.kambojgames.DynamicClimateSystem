using UnityEngine;
using UnityEngine.Rendering.Universal;
using KambojGames.Utilities2D;

[System.Serializable]
public class ClimateManager:MonoBehaviour 
{
    [SerializeField] DayNightCycleController m_dayAndNightController = new();

    private SeasonCycleController m_seasonCycleController = new();
    private Light2D m_globalLight;
    private Light2D m_sun;
    private Light2D m_moon;

    TimeAndDayManager m_timeAndDayManager;

    public void Awake()
    {
        ClimateData.Instance.StartWeather();
    }

    public void OnEnable()
    {
        m_globalLight = GameObject.FindGameObjectWithTag("GlobalLight")?.GetComponent<Light2D>();
        m_sun = GameObject.FindGameObjectWithTag("Sun")?.GetComponent<Light2D>();
        m_moon = GameObject.FindGameObjectWithTag("Moon")?.GetComponent<Light2D>();
        m_dayAndNightController.Initialize(m_sun, m_moon, m_globalLight);
        m_seasonCycleController.Initialize(m_sun, m_moon);

        m_timeAndDayManager = new TimeAndDayManager();
    }

    public void OnDisable()
    {
        m_dayAndNightController.Deinitialize();
    }

    public void Update()
    {
        m_timeAndDayManager.UpdateDateAndTime();
        m_dayAndNightController.UpdateNightDayCycle();
        m_seasonCycleController.UpdateSeasons();
        ClimateData.Instance.UpdateWeather();
    }

}
