using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KambojGames.Utilities2D;

public class CloudBehaviour : MonoBehaviour,ISpawnObject
{
    public GameObject ParentGameObject => gameObject;
    [SerializeField]
    private float m_moveSpeed = 5.0f;
    [SerializeField]
    private float m_cloudTravelDistance;
    [SerializeField]
    private string m_poolKey;


    private Vector2 m_spawnPos;
    public string PoolKey => m_poolKey;

    private void Awake()
    {
        m_spawnPos = transform.position;
    }
    

    void Update()
    {
        transform.position += (Vector3)(Vector2.left * m_moveSpeed * Time.deltaTime);

        float distance = Vector2.Distance(m_spawnPos, transform.position);
        if (distance >= m_cloudTravelDistance)
        {
            ObjectFactorySO.Instance.ReturnObjectToPool(m_poolKey, this);
        }
    }


}
