using UnityEngine;


public class GenericGazeBone : GazeBone
{
    [Tooltip("The Object with the transform to be used as the base transform for this bone")]
    [SerializeField]
    protected GameObject m_BaseTransformObject;

    protected Quaternion m_LocalAdditionalGazeRotation;
    protected Quaternion m_LocalAnimationRotation;

    [SerializeField]
    [Tooltip("The bone specific offset from the target location in local space")]
    protected Vector3 m_TargetOffset;

    [Tooltip("The max angle the bone can turn per second")]
    [SerializeField]
    protected float m_MaxTurnSpeed = 90.0f;

    [Tooltip("The max angles the bone can turn clockwise")]
    [SerializeField]
    protected Vector3 m_MaxAngles = Vector3.zero;

    [Tooltip("The max angles the bone can turn counter-clockwise")]
    [SerializeField]
    protected Vector3 m_MinAngles = Vector3.zero;

    protected bool m_ReachedMaxXAxisAngle = false;
    protected bool m_ReachedMaxYAxisAngle = false;
    protected bool m_ReachedMaxZAxisAngle = false;

    protected virtual void Start()
    {
        if (!m_BaseTransformObject)
        {
            m_BaseTransformObject = new GameObject(name + "_BaseTransformObject");
            m_BaseTransformObject.transform.parent = transform.parent;
            m_BaseTransformObject.transform.localPosition = transform.localPosition;
            m_BaseTransformObject.transform.localRotation = transform.localRotation;
        }

        m_LocalAdditionalGazeRotation = Quaternion.identity;
        m_LocalAnimationRotation = Quaternion.identity;
    }

    public override void UpdateGazeBoneRotation()
    {
        m_LocalAnimationRotation = transform.localRotation;
        transform.localRotation *= m_LocalAdditionalGazeRotation;
    }
    public void RotateBone(Vector3 directionToFace)
    {
        transform.rotation = Quaternion.LookRotation(directionToFace);

        LimitEulerRotation();

        m_LocalAdditionalGazeRotation = Quaternion.Inverse(m_LocalAnimationRotation) * transform.localRotation;
    }

    protected void LimitEulerRotation()
    {
        Vector3 euler = transform.localEulerAngles;
        Vector3 eulerBase = m_BaseTransformObject.transform.localEulerAngles;

        //Limitierung der Rotationen auf Werte zwischen -180.0 und 180.0
        if (euler.x > 180.0f) { euler.x -= 360.0f; }
        if (euler.y > 180.0f) { euler.y -= 360.0f; }
        if (euler.z > 180.0f) { euler.z -= 360.0f; }
        if (eulerBase.x > 180.0f) { eulerBase.x -= 360.0f; }
        if (eulerBase.y > 180.0f) { eulerBase.y -= 360.0f; }
        if (eulerBase.z > 180.0f) { eulerBase.z -= 360.0f; }
        //Berechnung der minimalen und maximalen Winkel
        Vector3 min = new Vector3(eulerBase.x + m_MinAngles.x, eulerBase.y + m_MinAngles.y, eulerBase.z + m_MinAngles.z);
        Vector3 max = new Vector3(eulerBase.x + m_MaxAngles.x, eulerBase.y + m_MaxAngles.y, eulerBase.z + m_MaxAngles.z);
        //Limitation der Rotation entlang der X-Achse
        m_ReachedMaxXAxisAngle = false;
        if (euler.x < min.x)
        {
            euler.x = min.x;
            m_ReachedMaxXAxisAngle = true;
        }
        else if (euler.x > max.x)
        {
            euler.x = max.x;
            m_ReachedMaxXAxisAngle = true;
        }

        //Limitation der Rotation entlang der Y-Achse
        m_ReachedMaxYAxisAngle = false;
        if (euler.y < min.y)
        {
            euler.y = min.y;
            m_ReachedMaxYAxisAngle = true;
        }
        else if (euler.y > max.y)
        {
            euler.y = max.y;
            m_ReachedMaxYAxisAngle = true;
        }

        //Limitation der Rotation entlang der Z-Achse
        m_ReachedMaxZAxisAngle = false;
        if (euler.z < min.z)
        {
            euler.z = min.z;
            m_ReachedMaxZAxisAngle = true;
        }
        else if (euler.z > max.z)
        {
            euler.z = max.z;
            m_ReachedMaxZAxisAngle = true;
        }

        transform.localEulerAngles = euler;
    }

    public override void LookAtGazeTarget(Vector3 targetLocation, bool previousGazeBonesReachedMaxAngle = true)
    {
        Vector3 directionThisFrame = GetTargetDirectionThisFrame(targetLocation);

        if(previousGazeBonesReachedMaxAngle)
        {
            RotateBone(directionThisFrame);
        }
        else
        {
            if (IsDirectionCloserToBaseTransform(directionThisFrame))
            {
                RotateBone(directionThisFrame);
            }
        }
    }

    public Vector3 GetOffsetTargetDirection(Vector3 targetLocation)
    {
        //Offset im lokalen Raum des Knochens
        Vector3 offset = 
              transform.right * m_TargetOffset.x  + 
              transform.up * m_TargetOffset.y  + 
              transform.forward * m_TargetOffset.z;
        return targetLocation + offset - transform.position;
    }

    public Vector3 GetTargetDirectionThisFrame(Vector3 targetLocation)
    {
        Vector3 possibleDirection = GetOffsetTargetDirection(targetLocation);
        //Wäre die Rotation zur Zielrichung um die X-Achse des Knochens größer als 90 Grad?
        if(Mathf.Abs(Vector3.SignedAngle(transform.forward, possibleDirection, transform.right)) > 90.0f)
        {
            float angleYAxis = Vector3.SignedAngle(transform.forward, possibleDirection, transform.up);
            possibleDirection = Quaternion.AngleAxis(angleYAxis, transform.up) * transform.forward;
        }

        float maxRadiansDelta = m_MaxTurnSpeed * Mathf.Deg2Rad * Time.deltaTime;
        return Vector3.RotateTowards(transform.forward, possibleDirection, maxRadiansDelta, 0.0f);
    }

    public bool IsDirectionCloserToBaseTransform(Vector3 directionToTest)
    {
        float angleCurrent = Vector3.Angle(m_BaseTransformObject.transform.forward, transform.forward);
        float angleDirection = Vector3.Angle(m_BaseTransformObject.transform.forward, directionToTest);
        if (angleDirection < angleCurrent)
        {
            return true;
        }
        return false;
    }

    public override bool ReachedMaxXAxisAngle()
    {
        return m_ReachedMaxXAxisAngle;
    }

    public override bool ReachedMaxYAxisAngle()
    {
        return m_ReachedMaxYAxisAngle;
    }

    public override bool ReachedMaxZAxisAngle()
    {
        return m_ReachedMaxZAxisAngle;
    }
}

