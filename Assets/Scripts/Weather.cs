using UnityEngine;


[CreateAssetMenu(fileName = "Weather", menuName = "ScriptableObject/WeatherData")]
public class Weather : ScriptableObject
{
    [SerializeField] private WeatherType m_weatherType;
    [SerializeField] private WeatherCondition m_weatherConditions;
    [Tooltip("Time range  to check the probability of hitting the weather effect. Range should be in hours and 24 hour format, this will not be real time but game time.")]
    [SerializeField] private MinMaxFloat m_weatherTimeRange;

    private Timer m_weatherCoolDownTimer; //after selecting weather add a cooldown value
    private float m_weatherStartTime;
    private float m_weatherEndTime;

    public WeatherType WeatherType => m_weatherType;
    public MinMaxFloat WeatherTimeRange => m_weatherTimeRange;

   

    private void OnValidate()
    {
        if(m_weatherTimeRange.min <0)
        {
            Debug.LogError("Minimum value of Weather Time Range cannot be 0");
        }
    }

    private void StartWeatherEvent()
    {
        float MinutesToLastADay = ClimateData.Instance.MinutesToLastADay;
        float secondsPerInGameHour = (MinutesToLastADay / 24f) * 60f;
        float MinRealTimeInSeconds = WeatherTimeRange.min * secondsPerInGameHour;
        float MaxRealTimeInSeconds = WeatherTimeRange.max * secondsPerInGameHour;

        m_weatherStartTime = MinRealTimeInSeconds;
        m_weatherEndTime = MaxRealTimeInSeconds;

        m_weatherConditions.SelectWeatherEffect(m_weatherStartTime,m_weatherEndTime);
    }

    public void UpdateWeather()
    {
        m_weatherCoolDownTimer.Update(Time.deltaTime);


        if (!m_weatherConditions.IsWeatherActive())
        {
            StartWeatherEvent();
        }

    }


    public WeatherBehaviour GetCurrentWeatherBehaviour()
    {
        return m_weatherConditions.GetCurrentWeatherBehaviour();
    }

}






