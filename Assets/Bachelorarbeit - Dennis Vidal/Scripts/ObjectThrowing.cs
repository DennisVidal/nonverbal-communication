using System.Collections.Generic;
using UnityEngine;

public class ObjectThrowing : MonoBehaviour
{
    [SerializeField]
    protected List<GameObject> m_ObjectToThrowList;

    [SerializeField]
    protected Vector3 m_ThrowOriginOffset;
    [SerializeField]
    protected Vector3 m_ThrowDirectionOffset;
    [SerializeField]
    protected float m_ThrowForce;
    [SerializeField]
    protected ForceMode m_ThrowForceMode = ForceMode.VelocityChange;

    [SerializeField]
    protected float m_BetweenThrowsDelay;
    protected float m_BetweenThrowsDelayTimer = -1.0f;


    void Start()
    {
        GameManager.Instance.OnAirTappedNotOnCharacter += OnAirTappedNotOnCharacter;
    }

    void Update()
    {
        if(m_BetweenThrowsDelayTimer > 0.0f)
        {
            m_BetweenThrowsDelayTimer -= Time.deltaTime;
        }
    }

    void OnAirTappedNotOnCharacter(Vector3 hitPosition)
    {
        if(GameManager.Instance.HasPlacedCharacter())
        {
            if (m_BetweenThrowsDelayTimer <= 0.0f)
            {
                m_BetweenThrowsDelayTimer = m_BetweenThrowsDelay;
                if (m_ObjectToThrowList.Count > 0)
                {
                    Vector3 actualOrigin = transform.position + transform.TransformDirection(m_ThrowOriginOffset);

                    Vector3 actualDirection;
                    if (hitPosition == Vector3.zero)
                    {
                        actualDirection = (transform.forward + m_ThrowDirectionOffset).normalized;
                    }
                    else
                    {
                        actualDirection = ((hitPosition - actualOrigin).normalized + m_ThrowDirectionOffset).normalized;
                    }


                    GameObject spawnedObject = Instantiate(GetRandomObjectToThrow(), actualOrigin, Quaternion.identity);
                    spawnedObject.transform.rotation = Quaternion.LookRotation(actualDirection);
                    Rigidbody rigidbody = spawnedObject.GetComponent<Rigidbody>();
                    if (rigidbody)
                    {
                        rigidbody.AddForce(actualDirection * m_ThrowForce, m_ThrowForceMode);
                    }
                }
            }
        }
    }

    protected GameObject GetRandomObjectToThrow()
    {
        if(m_ObjectToThrowList.Count > 0)
        {
            return m_ObjectToThrowList[Random.Range(0, m_ObjectToThrowList.Count)];
        }
        return null;
    }

    void OnDestroy()
    {
        GameManager.Instance.OnAirTappedNotOnCharacter -= OnAirTappedNotOnCharacter;
    }
}
