using System;
using UnityEngine;

public class ShadowInstance : MonoBehaviour
{
    [SerializeField]
    [Range(0, 10f)]
    private float m_baseLength = 1f;

    public float BaseLength => m_baseLength;

    private void OnEnable()
    {
        ClimateData.Instance.RegisterShadow(this);
    }

    private void OnDisable()
    {
        ClimateData.Instance.UnregisterShadow(this);
    }

   
}