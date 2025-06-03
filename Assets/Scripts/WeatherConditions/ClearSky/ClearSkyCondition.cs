using UnityEngine;
using KambojGames.Utilities2D.Attributes;


public class ClearSkyCondition : WeatherCondition
{
    [Header("ClearSky")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float m_probabilityOfClouds = 0.4f;
    [Tooltip("Range to check probaility of having clouds, Note: Time range should be in game time")]
    [SerializeField] private MinMaxFloat m_checkCloudProbabailityTime;

    [Header("Weather Objects")]
    [SerializeField] private Weather m_cloudWeather;

    private Timer m_updateWeatherTimer = new Timer(0.0f, 0.0f, null, null);

    private void OnValidate()
    {
        if (m_checkCloudProbabailityTime.min < 0)
        {
            Debug.LogError("Minimum value of the check cloud probability should be greater then 0");
        }
    }
    public override void Initialize()
    {
        base.Initialize();
        if (m_cloudWeather)
        {
            m_cloudWeather.Initialize();
        }

    }

    public override void UpdateCondition()
    {
       // if (!IsWeatherActive()) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Clear Sky");
        }


        if (!m_updateWeatherTimer.IsTimerRunning())
        {
            m_updateWeatherTimer.Reset();
            m_updateWeatherTimer.Start();
        }

        m_updateWeatherTimer.Update(Time.deltaTime);

      
    }


    protected override void OnWeatherSelected()
    {
        if (!IsWeatherActive()) return;

        ClimateData.SetCloudStrength(0.0f);
        float weatherUpdateGameTime = Random.Range(m_checkCloudProbabailityTime.max, m_checkCloudProbabailityTime.min);
        float weatherUpdateRealTime = ClimateData.CovertGameHoursToRealTimeInSecs(weatherUpdateGameTime);
        m_updateWeatherTimer = new Timer(1.0f, weatherUpdateRealTime, null, AddClouds);
        m_updateWeatherTimer.Start();


    }

    protected override void OnWeatherEnd() { StopInterpolator(); }


    private void AddClouds()
    {
        if (!ClimateData.IsRunningWeatherTypeWithBehaviour(WeatherType.Cloudy, WeatherBehaviour.None))
        {
            float probability = Random.Range(0.0f, 1.0f);
            if (probability <= m_probabilityOfClouds)
            {
                ClimateData.Instance.AddWeatherObject(m_cloudWeather);

                Debug.Log("Added Clouds");
            }
        }
       
    }

}