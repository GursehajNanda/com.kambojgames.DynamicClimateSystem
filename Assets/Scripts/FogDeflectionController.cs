using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FogDeflectionController : MonoBehaviour
{
    [SerializeField]
    private VisualEffect m_fog;

 

    void Update()
    {

        m_fog.SetVector3("ColliderPos", transform.position);
    }
}
