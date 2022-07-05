using UnityEngine;

public class FocusPoint : MonoBehaviour
{
    protected Vector3 m_Position;
    protected float m_TimeSinceLastChange = 0.0f;
    [SerializeField]
    protected float m_TimeToBeRecent = 2.0f;
    [SerializeField]
    protected float m_TimeToBeTooOld = 10.0f;

    void Update()
    {
        m_TimeSinceLastChange += Time.deltaTime;
    }
    public Vector3 GetPosition()
    {
       return m_Position;
    }
    public void SetPosition(Vector3 position)
    {
        m_Position = position;
        m_TimeSinceLastChange = 0.0f;
    }
   
    public bool IsRecent()
    {
        if(m_TimeSinceLastChange < m_TimeToBeRecent)
        {
            return true;
        }
        return false;
    }

    public bool IsTooOld()
    {
        if (m_TimeSinceLastChange >= m_TimeToBeTooOld)
        {
            return true;
        }
        return false;
    }
}
