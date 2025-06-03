using UnityEngine.VFX;
using UnityEngine;

public class ThunderWeatherCondition : WeatherCondition
{
    private VisualEffect m_thunerVfx;

    [Header("Weather Objects")]
    [SerializeField] private Weather m_thunderWeather;

    public override void Initialize()
    {
        base.Initialize();
        GameObject thunderObject = GameObject.FindGameObjectWithTag("Thunder");
        if (thunderObject)
        {
            m_thunerVfx = thunderObject?.GetComponent<VisualEffect>();
            if (!m_thunerVfx)
            {
                Debug.LogError("Thunder VFX is missing");
            }

            m_thunerVfx.Stop();
        }
        else
        {
            Debug.LogError("Could not find thunder vfx object");
        }

        OnWeatherConditionFailed += DisableWeather;
    }

    public override void UpdateCondition()
    {

        if (!IsWeatherActive()) return;

        base.UpdateCondition();
      
    }

    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;
        m_thunerVfx.Play();
        m_thunerVfx.SetBool("Lightnings", true);
        Debug.Log("Thunder");
    }

    protected override void OnWeatherEnd()
    {
        base.OnWeatherEnd();
        m_thunerVfx.SetBool("Lightnings", false);
       
    }

    private void DisableWeather()
    {
        RemoveWeather(m_thunderWeather);
        m_thunerVfx.Stop();
    }


}