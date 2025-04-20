using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//execute late to be sure the manager are instantiated 
public class ShadowInstance : MonoBehaviour
{
    [SerializeField]
    [Range(0, 10f)]
    private float m_baseLength = 1f;

    public float BaseLength => m_baseLength;

    private void OnEnable()
    {
        ClimateDataSO.Instance.RegisterShadow(this);
    }

    private void OnDisable()
    {
        ClimateDataSO.Instance.UnregisterShadow(this);
    }
}