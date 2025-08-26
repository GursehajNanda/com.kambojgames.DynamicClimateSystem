using System.Collections.Generic;
using UnityEngine;
using KambojGames.Utilities2D;
using KambojGames.Utilities2D.Attributes;
using System;

[Serializable]
public class CloudController 
{
    [SerializeField]
    private List<CloudBehaviour> m_clouds;
    [SerializeField]
    private float m_spawnRate = 4.0f;
  
   

    private Timer m_spawnTimer;
    private List<string> m_cloudsKeys = new();
    private bool m_startSpawn;
    private Camera cam;
    public void Initialize()
    {
        cam = Camera.main;

        foreach (CloudBehaviour cloud in m_clouds)
        {
            ObjectFactorySO.Instance.CreateObjectPool(cloud.PoolKey, cloud, true, 3, 6,true);
            m_cloudsKeys.Add(cloud.PoolKey);
        }

        m_spawnTimer = new Timer(1.0f, m_spawnRate, null, SpawnCloud);
        m_startSpawn = false;
    }

    public void StartSpawn()
    {
        m_startSpawn = true;
    }

    public void StopSpawn()
    {
        m_startSpawn = false;
        m_spawnTimer.Stop();
    }

    public void UpdateClouds()
    {
        if (!m_startSpawn) return;
        m_spawnTimer.Update(Time.deltaTime);
        if(!m_spawnTimer.IsTimerRunning())
        {
            m_spawnTimer.Reset();
            m_spawnTimer.Start();
        }
    }

    private void SpawnCloud()
    {
      
        int i = UnityEngine.Random.Range(0, m_cloudsKeys.Count);

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        float rightEdgeX = cam.transform.position.x + halfWidth + 3.0f;

      
        float topY = cam.transform.position.y + halfHeight;
        float bottomY = cam.transform.position.y - halfHeight;
        float spawnPosY = UnityEngine.Random.Range(bottomY, topY);

        Vector2 cloudSpawnPos = new Vector2(rightEdgeX, spawnPosY);

        ISpawnObject cloud = ObjectFactorySO.Instance?.GetObjectFromPool(m_cloudsKeys[i]);
        if(cloud == null)
        {
            foreach (CloudBehaviour cloudBehav in m_clouds)
            {
                m_cloudsKeys.Clear();
                ObjectFactorySO.Instance.CreateObjectPool(cloudBehav.PoolKey, cloud, true, 3, 6, true);
                m_cloudsKeys.Add(cloudBehav.PoolKey);
            }
        }
        else
        {
            cloud.ParentGameObject.transform.position = cloudSpawnPos;
        }
      
    }


}
