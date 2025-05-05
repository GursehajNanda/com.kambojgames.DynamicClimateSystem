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

 
    private AnimationCurve m_ambientCurve = new AnimationCurve(
        new Keyframe(0f, 0.1f),    // Midnight
        new Keyframe(0.25f, 0.25f), // Dawn
        new Keyframe(0.5f, 0.4f),  // Noon
        new Keyframe(0.75f, 0.25f), // Dusk
        new Keyframe(1f, 0.1f)     // Night again
    );



    private AnimationCurve m_shadowLength = new AnimationCurve(
      new Keyframe(0f, 0.0f),  
      new Keyframe(0.1875f, 0.2f), 
      new Keyframe(0.25f, 0.4f),
      new Keyframe(0.5f, 1.0f),  
      new Keyframe(0.75f, 0.4f), 
      new Keyframe(0.89f, 0.0f)   
 );

 
    private AnimationCurve m_shadowAngle = new AnimationCurve(
     new Keyframe(0f, 0.5f),       //180°
     new Keyframe(0.25f, 0.75f),   //270°
     new Keyframe(0.5f, 1.0f),     //360°
     new Keyframe(0.75f, 1.25f),   //450°
     new Keyframe(1f, 1.5f)        //540° 
 );



    private ClimateData m_climateData;
    private Light2D m_sun;
    private Light2D m_moon;
    private Light2D m_globalLight;
    private Dictionary<Light2D, float> m_lightBaseIntensities = new();
    private List<ShadowInstance> m_shadows = new();
    private List<LightInterpolator> m_lightBlenders = new();
    private DayPeriod m_currentPeriod;
    private float m_inGameTime; // 0 - 24
    private bool m_isInitialzied = false;


    public void Initialize(Light2D sun, Light2D moon,Light2D globalLight)
    {
        m_sun = sun;
        m_moon = moon;
        m_globalLight = globalLight;

        foreach (var light in m_nightLights)
        {
            if (!m_lightBaseIntensities.ContainsKey(light))
            {
                m_lightBaseIntensities[light] = light.intensity;
            }
        }

        m_climateData = ClimateData.Instance;
        m_shadows = m_climateData.Shadows;
        m_lightBlenders = m_climateData.LightBlender;
        m_isInitialzied = true;

    }


    public void UpdateNightDayCycle()
    {
        if (!m_isInitialzied) { Debug.LogError("DayNightCycleController is not Initialzed, Please Initialize it correctly"); }

        DateTime currentTime = m_climateData.GetDateTimeYearData();

        // Convert hour + minute into float (e.g., 14.5f for 2:30 PM)
        m_inGameTime = currentTime.Hour + (currentTime.Minute / 60f);
        float t = m_inGameTime / 24f;

        UpdateSunPosition();
        UpdateMoonPosition();
        UpdateDayPeriod();
        UpdateNightLights(t);
        UpdateAmbientIntensity(t);
        UpdateShadow(t);
    }

    void UpdateSunPosition()
    {
        float angle = ((m_inGameTime / 24f) * 360f) - 90f; 
        Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * m_orbitRadius;
        m_sun.transform.position = m_orbitCenter.position + offset;
        m_sun.transform.right = (m_orbitCenter.position - m_sun.transform.position).normalized;
    }


    void UpdateMoonPosition()
    {
        float moonAngle = ((m_inGameTime / 24f) * 360f) + 90f;
        Vector3 moonOffset = new Vector3(Mathf.Cos(moonAngle * Mathf.Deg2Rad), Mathf.Sin(moonAngle * Mathf.Deg2Rad)) * m_orbitRadius;
        m_moon.transform.position = m_orbitCenter.position + moonOffset;
        m_moon.transform.right = (m_orbitCenter.position - m_moon.transform.position).normalized;
    }

    void UpdateNightLights(float ratio)
    {
        float cloudStrength = m_climateData.CloudsStrength;
        float fadeFactor = Mathf.Clamp01(m_nightLightFadeCurve.Evaluate(ratio));

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

                light.intensity += cloudStrength * 0.5f;
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



        m_climateData.SetDayPeriod(m_currentPeriod);

    }

    void UpdateShadow(float ratio)
    {
        float currentShadowAngle = m_shadowAngle.Evaluate(ratio);
        float opposedAngle = Mathf.Repeat(currentShadowAngle + 0.5f, 1f);
        float currentShadowLength = m_shadowLength.Evaluate(ratio);
        float rotationDegrees = opposedAngle * 360f;

        foreach (var shadow in m_shadows)
        {
            var t = shadow.transform;
            t.rotation = Quaternion.AngleAxis(rotationDegrees, Vector3.forward);
            t.localScale = new Vector3(1, shadow.BaseLength * currentShadowLength, 1);
        }
      
        foreach (var handler in m_lightBlenders)
        {
            handler.SetRatio(ratio);
        }
    }

    void UpdateAmbientIntensity(float ratio)
    {
        float ambientTargetIntensity = m_ambientCurve.Evaluate(ratio);
        m_globalLight.intensity = Mathf.Lerp(m_globalLight.intensity, ambientTargetIntensity, Time.deltaTime * m_intensitySmoothSpeed);
    }
  
   
}
