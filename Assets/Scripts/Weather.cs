using UnityEngine;


[CreateAssetMenu(fileName = "Weather", menuName = "ScriptableObject/WeatherData")]
public class Weather : ScriptableObject
{
    [SerializeField] private WeatherType m_weatherType;
    [SerializeField] private WeatherCondition m_weatherConditions;
    [SerializeField] private MinMaxFloat m_weatherTimeRange;

    private Timer m_weatherCooldownTimer;

    public WeatherType WeatherType => m_weatherType;
    [Tooltip("Range should be in hours and 24 hour format, this will not be real time but game time")]
    public MinMaxFloat WeatherTimeRange => m_weatherTimeRange;

   

    private void OnValidate()
    {
        if(m_weatherTimeRange.min <0)
        {
            Debug.LogError("Minimum value of Weather Time Range cannot be 0");
        }
    }

    public void UpdateWeather()
    {
        m_weatherCooldownTimer.Update(Time.deltaTime);
        if(IsWeatherCleared())
        {
            m_weatherCooldownTimer.Reset();
            ClimateData.Instance.RemoveWeather(this);
        }
    }

    public void StartWeather()
    {
        if (m_weatherConditions.StartWeatherEffect())
        {
            if (!m_weatherCooldownTimer.IsTimerRunning())
            {
                float MinutesToLastADay = ClimateData.Instance.MinutesToLastADay;
                float secondsPerInGameHour = (MinutesToLastADay / 24f) * 60f;
                float MinRealTimeInSeconds = WeatherTimeRange.min * secondsPerInGameHour;
                float MaxRealTimeInSeconds = WeatherTimeRange.max * secondsPerInGameHour;

                float nextWeatherSelectionTime = Random.Range(MinRealTimeInSeconds, MaxRealTimeInSeconds);

                m_weatherCooldownTimer = new Timer(0f, nextWeatherSelectionTime, null, StartClearingWeather);
                m_weatherCooldownTimer.Start();
            }
        }
    }

    public void StartClearingWeather()
    {
        m_weatherConditions.StartClearingWeather();
    }

    public bool IsWeatherCleared()
    {
        return m_weatherConditions.GetIsWeatherCleared();
    }

    public WeatherBehaviour GetCurrentWeatherBehaviour()
    {
        return m_weatherConditions.GetCurrentWeatherBehaviour();
    }

}






