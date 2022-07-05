using System;
using UnityEngine;

public class LookAtEvent : MonoBehaviour
{
    [SerializeField]
    protected float m_BaseLookAtEventProbability = 0.7f;

    [SerializeField]
    public event Action<LookAtEvent> OnLookAtEvent;

    protected void Start()
    {
        GameManager.Instance.RegisterLookAtEvent(this);
    }

    protected bool TriggerLookAtEvent()
    {
        if (OnLookAtEvent != null)
        {
            OnLookAtEvent(this);
            return true;
        }
        return false;
    }
    public virtual Vector3 GetPosition()
    {
        return transform.position;
    }

    public virtual float GetLookAtEventProbability()
    {
        return m_BaseLookAtEventProbability;
    }

    protected void OnDestroy()
    {
        GameManager.Instance.UnregisterLookAtEvent(this);
    }
}
