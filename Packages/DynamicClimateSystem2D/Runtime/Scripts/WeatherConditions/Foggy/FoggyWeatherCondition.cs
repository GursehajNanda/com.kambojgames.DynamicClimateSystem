
using UnityEngine.VFX;
using UnityEngine;

public class FoggyWeatherCondition : WeatherCondition
{
    [Header("Fog")]
    [SerializeField] float m_fogAmount;
    private VisualEffect m_fogVfx;

    public override void Initialize()
    {
        base.Initialize();
        GameObject fogObject = GameObject.FindGameObjectWithTag("Fog");
        if (fogObject)
        {
            m_fogVfx = fogObject?.GetComponent<VisualEffect>();
            if (!m_fogVfx)
            {
                Debug.LogError("Fog VFX is missing");
            }

            m_fogVfx.Stop();
        }
        else
        {
            Debug.LogError("Could not find fog vfx object");
        }
    }



    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;
        m_fogVfx.Play();
        m_fogVfx.SetFloat("FogAmount", m_fogAmount);
    }

    protected override void OnWeatherEnd()
    {
        base.OnWeatherEnd();
        RemoveWeather(null);
        m_fogVfx.Stop();
    }
    

   
}