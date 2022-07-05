using UnityEngine;

public enum InterpersonalDistanceZoneType
{
    None = 0,
    Intimate,
    Personal,
    Social,
    Public
}

public class InterpersonalDistanceZone : MonoBehaviour
{
    [Tooltip("The zone type of this distance zone")]
    [SerializeField]
    protected InterpersonalDistanceZoneType m_Type;

    [Tooltip("The lower border of this zone")]
    [SerializeField]
    protected float m_DistanceMin;

    [Tooltip("The upper border of this zone")]
    [SerializeField]
    protected float m_DistanceMax;

    [Tooltip("Probability of mutual gaze in this zone")]
    [Range(0.0f, 1.0f)]
    [SerializeField]
    protected float m_MutualGazeProbability = 1.0f;

    public float GetMutualGazePobability()
    {
        return m_MutualGazeProbability;
    }

    public float GetMutualGazePobabilityDistance()
    {
        float min = GetMinDistance();
        float max = GetMaxDistance();

        if (max < 0.0f)
        {
            return min;
        }

        return min + (max - min) / 2;
    }

    public float GetMinDistance()
    {
        if(m_DistanceMin < 0.0f)
        {
            return 0.0f;
        }
        return m_DistanceMin;
    }
    public float GetMaxDistance()
    {
        if (m_DistanceMax < 0.0f)
        {
            return -1.0f;
        }
        return m_DistanceMax;
    }

    public bool IsPositionInZone(Vector3 zoneTargetPosition, Vector3 positionToTest, float offsetRadius = 0.0f)
    {
        Vector3 zoneTargetPositionXZ = new Vector3(zoneTargetPosition.x, 0.0f, zoneTargetPosition.z);
        Vector3 positionToTestXZ = new Vector3(positionToTest.x, 0.0f, positionToTest.z);
        float distanceToPosition = Vector3.Distance(positionToTestXZ, zoneTargetPositionXZ);
        if (GetMinDistance() + offsetRadius < distanceToPosition && distanceToPosition < GetMaxDistance() + offsetRadius)
        {
            return true;
        }
        return false;
    }
}