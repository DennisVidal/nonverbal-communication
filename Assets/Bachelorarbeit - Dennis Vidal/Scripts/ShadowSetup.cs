using UnityEngine;

public class ShadowSetup: MonoBehaviour
{
    [SerializeField]
    protected GameObject m_ShadowObject;
    [SerializeField]
    protected float m_MaxDistance = 1.0f;

    void Update()
    {
        if(m_ShadowObject)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, m_MaxDistance, 1 << 31))
            {
                if (!m_ShadowObject.activeInHierarchy)
                {
                    m_ShadowObject.SetActive(true);
                }
                m_ShadowObject.transform.position = hit.point;
                m_ShadowObject.transform.rotation = Quaternion.LookRotation(Vector3.up);
            }
            else
            {
                if (m_ShadowObject.activeInHierarchy)
                {
                    m_ShadowObject.SetActive(false);
                }
            }
        }
    }
}
