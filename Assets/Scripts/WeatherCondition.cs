using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine;


public abstract class WeatherCondition : MonoBehaviour
{
    [Tooltip("Please ensure total sum of probabilities for effects should be 1")]
    [SerializeField]private List<WeatherEffects> m_weatherEffects;
    [SerializeField]private WeatherType m_conditionForWeather;

    protected WeatherEffects CurrentWeatherEffect;

    private void OnValidate()
    {
        int totalProbability = 0;
        foreach(WeatherEffects weatherEffects in m_weatherEffects)
        {
            totalProbability += weatherEffects.Probability;
        }

        if(totalProbability >1.0)
        {
            Debug.LogError("Please ensure total sum of probabilities for effects should be 1");
        }
    }


    protected  void SelectWeatherEffect()
    {
        if (!IsConditionMet()) return;

        float randomProbability = Random.Range(0f, 1f);
        float currentProbability = 0f;

        foreach (var weathereffect in m_weatherEffects)
        {
            currentProbability += weathereffect.Probability;
            if (randomProbability < currentProbability)
            {
                CurrentWeatherEffect = weathereffect;
                break;
            }
        }

    }

    private  bool IsConditionMet()
    {
        if(m_conditionForWeather == ClimateData.Instance.CurrentWeather.WeatherType)
        {
            return true;
        }

        return false;
    }

    public abstract void StartWeatherEffect();

}

[System.Serializable]
public struct WeatherEffects
{
    private ParticleSystem m_effect;
    private VisualEffect m_vFX;
    private WeatherBehaviour m_weatherBehavior;
    [Range(0f, 1f)]
    private int m_probability;

    public ParticleSystem Effect => m_effect;
    public VisualEffect VFX=> m_vFX;
    public WeatherBehaviour WeatherBehavior => m_weatherBehavior;
    public int Probability =>m_probability;
}

public enum WeatherBehaviour
{
    None,
    Normal,
    Moderate,
    Heavy
}

public enum WeatherType
{
    None,
    Clear,
    Cloudy,
    Rainy,
    Windy,
    Foggy,
    Thunder,
}
