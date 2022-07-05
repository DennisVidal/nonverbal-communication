using UnityEngine;

public class HitSomethingLookAtEvent : LookAtEvent
{
    protected float m_LastCollisionVelocity = 0.0f;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag != "Player" && collision.collider.tag != "Character")
        {
            m_LastCollisionVelocity = collision.relativeVelocity.magnitude;
            if (m_LastCollisionVelocity > 0.2f)
            {
                TriggerLookAtEvent();
            }
        }
    }
    public override float GetLookAtEventProbability()
    {
        float probability = Mathf.Clamp01(m_LastCollisionVelocity);
        if (probability < m_BaseLookAtEventProbability)
        {
            probability = m_BaseLookAtEventProbability;
        }    
        return probability;
    }
}
