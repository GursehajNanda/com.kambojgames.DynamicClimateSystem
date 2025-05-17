using UnityEngine.VFX;
using UnityEngine;

public class FoggyWeatherCondition : WeatherCondition
{
    [Header("Fog")]
    [SerializeField] float m_fogAmount;

    private GameObject m_fogObject;
    private VisualEffect m_fogVfx;

    public override void Initialize()
    {
        base.Initialize();
        m_fogObject = GameObject.FindGameObjectWithTag("Fog");
        if (m_fogObject)
        {
            m_fogObject.SetActive(false);
            m_fogVfx = m_fogObject?.GetComponent<VisualEffect>();
            if (!m_fogVfx)
            {
                Debug.LogError("Fog VFX is missing");
            }
        }
        else
        {
            Debug.LogError("Could not find fog vfx object");
        }
    }



    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;
        m_fogObject.SetActive(true);
        m_fogVfx.SetFloat("FogAmount", m_fogAmount);
    }

    protected override void OnWeatherEnd()
    {
        m_fogObject.SetActive(false);
        RemoveWeather();   
    }
}
