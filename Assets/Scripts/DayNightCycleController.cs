using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;


[Serializable]
public class DayNightCycleController 
{
    [SerializeField] private Transform m_orbitCenter;
    [SerializeField] private float m_orbitRadius = 20f;
    [SerializeField] float m_intensitySmoothSpeed = 2f;

    [Header("Night Lights")]
    [SerializeField] private List<Light2D> m_nightLights;

    [Header("Time Period Settings")]
    [SerializeField] private float m_morningStart = 4f;
    [SerializeField] private float m_afternoonStart = 12f;
    [SerializeField] private float m_eveningStart = 17f;
    [SerializeField] private float m_nightStart = 21f;


    private AnimationCurve m_nightLightFadeCurve = new AnimationCurve(
        new Keyframe(0f, 1f),           // Midnight - ON
        new Keyframe(0.167f, 0.7f),     // 04:00 — fading down
        new Keyframe(0.25f, 0f),        // 6 AM - lights fade OUT
        new Keyframe(0.75f, 0.4f),        // 6PM  — early evening (starting to rise) 
        new Keyframe(0.79f, 0.7f),        // ~7 PM  — getting brighter
        new Keyframe(1f, 1f)            // Midnight - ON
    );

    [SerializeField]
    private AnimationCurve m_ambientCurve = new AnimationCurve(
        new Keyframe(0f, 0.1f),    // Midnight
        new Keyframe(0.25f, 0.3f), // Dawn
        new Keyframe(0.5f, 0.5f),  // Noon
        new Keyframe(0.75f, 0.3f), // Dusk
        new Keyframe(1f, 0.1f)     // Night again
    );

    private Light2D m_sun;
    private Light2D m_globalLight;
    private Dictionary<Light2D, float> m_lightBaseIntensities = new Dictionary<Light2D, float>();
    private DayPeriod m_currentPeriod;
    private float m_inGameTime; // 0 - 24
    private bool m_isInitialzied = false;


    public void Initialize(Light2D sun,Light2D globalLight)
    {
        m_sun = sun;
        m_globalLight = globalLight;
       

        foreach (var light in m_nightLights)
        {
            if (!m_lightBaseIntensities.ContainsKey(light))
            {
                m_lightBaseIntensities[light] = light.intensity;
            }
        }

        m_isInitialzied = true;
    }


    public void UpdateNightDayCycle()
    {
        if (!m_isInitialzied) { Debug.LogError("DayNightCycleController is not Initialzed, Please Initialize it correctly"); }

        DateTime currentTime = ClimateDataSO.Instance.GetDateTimeYearData();

        // Convert hour + minute into float (e.g., 14.5f for 2:30 PM)
        m_inGameTime = currentTime.Hour + (currentTime.Minute / 60f);

        UpdateSunPosition();
        UpdateDayPeriod();
        UpdateNightLights();
        UpdateAmbientIntensity();
    }

    void UpdateSunPosition()
    {
        float angle = ((m_inGameTime / 24f) * 360f) - 90f; // Shift so noon is at top (90° up)
        Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * m_orbitRadius;
        m_sun.transform.position = m_orbitCenter.position + offset;
        m_sun.transform.right = (m_orbitCenter.position - m_sun.transform.position).normalized;
    }

   
    void UpdateNightLights()
    {
        float t = m_inGameTime / 24f;
        float fadeFactor = Mathf.Clamp01(m_nightLightFadeCurve.Evaluate(t));

        foreach (var light in m_nightLights)
        {
            if (m_lightBaseIntensities.TryGetValue(light, out float baseIntensity))
            {
                FlickerLight2D flickerLight = light?.GetComponent<FlickerLight2D>();
                if (flickerLight)
                {
                    light.intensity = flickerLight.CurrentIntensity * fadeFactor;
                }
                else
                {
                    light.intensity = baseIntensity * fadeFactor;
                }
                
            }
        }
    }

    void UpdateDayPeriod()
    {
        if (m_inGameTime >= m_nightStart || m_inGameTime < m_morningStart)
        {
            m_currentPeriod = DayPeriod.Night;
        }
        else if (m_inGameTime >= m_morningStart && m_inGameTime < m_afternoonStart)
        {
            m_currentPeriod = DayPeriod.Morning;
        }
        else if (m_inGameTime >= m_afternoonStart && m_inGameTime < m_eveningStart)
        {
            m_currentPeriod = DayPeriod.Afternoon;
        }
        else
        {
            m_currentPeriod = DayPeriod.Evening;
        }

       

        ClimateDataSO.Instance.SetDayPeriod(m_currentPeriod);

    }

    void UpdateAmbientIntensity()
    {
        float t = m_inGameTime / 24f;
        float ambientTargetIntensity = m_ambientCurve.Evaluate(t);
        m_globalLight.intensity = Mathf.Lerp(m_globalLight.intensity, ambientTargetIntensity, Time.deltaTime * m_intensitySmoothSpeed);
    }
  
   
}
