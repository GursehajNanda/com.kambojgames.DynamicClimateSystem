using UnityEngine.VFX;
using UnityEngine;

public class ThunderWeatherCondition : WeatherCondition
{
    private GameObject m_thunderObject;
    private VisualEffect m_thunerVfx;

    [Header("Weather Objects")]
    [SerializeField] private Weather m_thunderWeather;

    public override void Initialize()
    {
        base.Initialize();
        m_thunderObject = GameObject.FindGameObjectWithTag("Thunder");
        if (m_thunderObject)
        {
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

    public override void UpdateCondition()
    {
        base.UpdateCondition();
        if(!IsConditionMet())
        {
            RemoveWeather(m_thunderWeather);
        }
    }

    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;
        m_thunderObject.SetActive(true);
        m_thunerVfx.SetBool("Lightnings", true);
        Debug.Log("Thunder");
    }

    protected override void OnWeatherEnd()
    {
        base.OnWeatherEnd();
        RemoveWeather(m_thunderWeather);
    }

    public override void DeactivateWeather()
    {
        m_thunerVfx.SetBool("Lightnings", false);
        m_thunderObject.SetActive(false);
    }
}
