using System.Collections.Generic;
using UnityEngine;
using KambojGames.Utilities2D;
using System;

[Serializable]
public class CloudController 
{
    [SerializeField]
    private List<CloudBehaviour> m_clouds;
    [SerializeField]
    private float m_spawnRate = 4.0f;
    [SerializeField]
    private float m_cloudspawnPosX;
    [SerializeField]
    private Vector2 m_cloudSpawnYRange;

    private Timer m_spawnTimer;
    private List<string> m_cloudsKeys = new();

    public void Initialize()
    {
        foreach (CloudBehaviour cloud in m_clouds)
        {
            ObjectFactorySO.Instance.CreateObjectPool(cloud.PoolKey, cloud, true, 3, 6);
            m_cloudsKeys.Add(cloud.PoolKey);
        }

        m_spawnTimer = new Timer(1.0f, m_spawnRate, null, SpawnCloud);
        m_spawnTimer.Start();
    }

    public void UpdateClouds()
    {
        m_spawnTimer.Update(Time.deltaTime);
        if(!m_spawnTimer.IsTimerRunning())
        {
            m_spawnTimer.Reset();
            m_spawnTimer.Start();
        }
    }

    private void SpawnCloud()
    {
        Debug.Log("Spawn");

        int i = UnityEngine.Random.Range(0, m_cloudsKeys.Count);
        float spawnPosY = UnityEngine.Random.Range(m_cloudSpawnYRange.x, m_cloudSpawnYRange.y);
        Vector2 cloudSpawnPos = new Vector2(m_cloudspawnPosX, spawnPosY);

        ISpawnObject cloud = ObjectFactorySO.Instance.GetObjectFromPool(m_cloudsKeys[i]);

        cloud.ParentGameObject.transform.position = cloudSpawnPos;
    }


}
