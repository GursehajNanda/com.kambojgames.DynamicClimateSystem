using UnityEngine;
using UnityEngine.VFX;

public class FogDeflectionController : MonoBehaviour
{
    private VisualEffect m_fog;

    private void Awake()
    {
        m_fog = GameObject.FindGameObjectWithTag("Fog").GetComponent<VisualEffect>();
    }

    void Update()
    {
        if (m_fog == null) return;
        m_fog.SetVector3("ColliderPos", transform.position);
    }
}
