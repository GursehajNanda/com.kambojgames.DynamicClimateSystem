using UnityEngine.VFX;
using UnityEngine;

public class ThunderWeatherCondition : WeatherCondition
{
    private VisualEffect m_thunerVfx;


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
    }

    protected override void OnWeatherEnd()
    {
        base.OnWeatherEnd();
        m_thunerVfx.SetBool("Lightnings", false);
        DisableWeather();


    }

    private void DisableWeather()
    {
        RemoveWeather(null);
        m_thunerVfx.Stop();
    }


}