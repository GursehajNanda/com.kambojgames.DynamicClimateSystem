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

    private List<ParticleSystem> m_windsEffects = new List<ParticleSystem>();
    private ParticleSystem m_leafsEffect;



    public override void Initialize()
    {
        base.Initialize();
        GameObject windsObject = GameObject.FindGameObjectWithTag("Wind");
        if (windsObject)
        {
            ParticleSystem[] windEffects = windsObject?.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem effect in windEffects)
            {
                m_windsEffects.Add(effect);
                effect.Stop();
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

        GameObject m_leafsObject = GameObject.FindGameObjectWithTag("Leafs");
        if (m_leafsObject)
        {
            m_leafsEffect = m_leafsObject?.GetComponent<ParticleSystem>();
            if (!m_leafsEffect)
            {
                Debug.LogError("Leafs VFX is missing");
            }

            m_leafsEffect.Stop();
        }
        else
        {
            Debug.LogError("Could not find leafs vfx object");
        }

        OnWeatherConditionFailed += DisableWeather;
    }

    public override void UpdateCondition()
    {
        if (!IsWeatherActive()) return;

        base.UpdateCondition();

        if (ClimateData.IsRunningWeatherTypeWithBehaviour(WeatherType.Foggy, WeatherBehaviour.None))
        {
            DisableWeather();
        }
    }
    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;
        foreach (ParticleSystem effect in m_windsEffects)
        {
            effect.Play();
        }
    
        foreach (ParticleSystem wind in m_windsEffects)
        {
            SetParticleEmissionSpeed(wind, m_windSimulationSpeed);
        }
         
        
        if(m_haveLeafs)
        {
            m_leafsEffect.Play();
            SetParticleEmissionSpeed(m_leafsEffect, m_leafsSimulationSpeed);
            SetParticleEmissionRate(m_leafsEffect, m_leafsEmmisionOverTime);

            SetParticleStartColor(m_leafsEffect, ClimateData.GetVegetationColor());

        }

    }

    protected override void OnWeatherEnd()
    {
        base.OnWeatherEnd();
        DisableWeather();
    }

    private  void DisableWeather()
    {
        foreach (ParticleSystem effect in m_windsEffects)
        {
            effect.Stop();
        }

        m_leafsEffect.Stop();

        RemoveWeather(null);
    }

}