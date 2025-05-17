using UnityEngine.VFX;
using UnityEngine;

public class ThunderWeatherCondition : WeatherCondition
{
    private GameObject m_thunderObject;
    private VisualEffect m_thunerVfx;

    public override void Initialize()
    {
        base.Initialize();
        m_thunderObject = GameObject.FindGameObjectWithTag("Thunder");
        if (m_thunderObject)
        {
            m_thunderObject.SetActive(false);
            m_thunerVfx = m_thunderObject?.GetComponent<VisualEffect>();
            if (!m_thunerVfx)
            {
                Debug.LogError("Thunder VFX is missing");
            }
        }
        else
        {
            Debug.LogError("Could not find thunder vfx object");
        }
    }


    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;
        m_thunderObject.SetActive(true);
        m_thunerVfx.SetBool("Lightnings", true);
    }

    protected override void OnWeatherEnd()
    {
        m_thunerVfx.SetBool("Lightnings", false);
        m_thunderObject.SetActive(false);
        RemoveWeather();
    }
}
