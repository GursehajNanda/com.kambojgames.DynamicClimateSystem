using System.Collections.Generic;
using KambojGames.Utilities2D.Attributes;
using UnityEngine;

public class WindyWeatherCondition : WeatherCondition
{
    [Header("Winds")]
    [SerializeField] private float m_windSimulationSpeed;
    [Header("Leafs")]
    [SerializeField] bool m_haveLeafs;
    [ConditionalField("m_haveLeafs")]
    [SerializeField] private float m_leafsSimulationSpeed;
    [ConditionalField("m_haveLeafs")]
    [SerializeField] private float m_leafsEmmisionOverTime;

    private GameObject m_windsObject;
    private List<ParticleSystem> m_windsEffects = new List<ParticleSystem>();
    private GameObject m_leafsObject;
    private ParticleSystem m_leafsEffect;
    public Material targetMaterial;


    public override void Initialize()
    {
        base.Initialize();
        m_windsObject = GameObject.FindGameObjectWithTag("Wind");
        if (m_windsObject)
        {
            m_windsObject.SetActive(false);
            ParticleSystem[] windEffects = m_windsObject?.GetComponents<ParticleSystem>();
            foreach(ParticleSystem effect in windEffects)
            {
                m_windsEffects.Add(effect);
            }
         
            if (m_windsEffects.Count == 0)
            {
                Debug.LogError("Wind VFX is missing");
            }
        }
        else
        {
            Debug.LogError("Could not find winds vfx object");
        }

        m_leafsObject = GameObject.FindGameObjectWithTag("Leafs");
        if(m_leafsObject)
        {
            m_leafsObject.SetActive(false);
            m_leafsEffect = m_leafsObject?.GetComponent<ParticleSystem>();
            if (!m_leafsEffect)
            {
                Debug.LogError("Leafs VFX is missing");
            }
        }
        else
        {
            Debug.LogError("Could not find leafs vfx object");
        }
    }


    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;
        m_windsObject.SetActive(true);
        m_leafsObject.SetActive(true);

        foreach (ParticleSystem wind in m_windsEffects)
        {
            SetParticleEmissionSpeed(wind, m_windSimulationSpeed);
        }
        SetParticleEmissionSpeed(m_leafsEffect, m_leafsSimulationSpeed);
        SetParticleEmissionRate(m_leafsEffect, m_leafsEmmisionOverTime);

        SetParticleStartColor(m_leafsEffect, ClimateData.Instance.GetVegetationColor());

    }

    protected override void OnWeatherEnd()
    {
        m_windsObject.SetActive(false);
        m_leafsObject.SetActive(false);
        RemoveWeather();
    }

   
}
