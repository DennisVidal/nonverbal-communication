using UnityEngine;

public class EyesGazeBone : GenericGazeBone
{
    [Tooltip("The bone object of the second eye")]
    [SerializeField]
    protected GameObject m_OtherEye;

    [Tooltip("The Object with the transform to be used as the base transform of the other eye")]
    [SerializeField]
    protected GameObject m_BaseTransformObjectOtherEye;

    protected Quaternion m_LocalAdditionalGazeRotationOtherEye;
    protected Quaternion m_LocalAnimationRotationOtherEye;

    protected bool m_ReachedMaxXAxisAngleOtherEye = false;
    protected bool m_ReachedMaxYAxisAngleOtherEye = false;
    protected bool m_ReachedMaxZAxisAngleOtherEye = false;

    protected override void Start()
    {
        base.Start();
        if(m_OtherEye)
        {
            if (!m_BaseTransformObjectOtherEye)
            {
                m_BaseTransformObjectOtherEye = new GameObject(m_OtherEye.name + "_BaseTransformObject");
                m_BaseTransformObjectOtherEye.transform.parent = m_OtherEye.transform.parent;
                m_BaseTransformObjectOtherEye.transform.localPosition = m_OtherEye.transform.localPosition;
                m_BaseTransformObjectOtherEye.transform.localRotation = m_OtherEye.transform.localRotation;
            }

            m_LocalAdditionalGazeRotationOtherEye = Quaternion.identity;
            m_LocalAnimationRotationOtherEye = Quaternion.identity;
        }
    }

    public override void UpdateGazeBoneRotation()
    {
        base.UpdateGazeBoneRotation();

        m_LocalAnimationRotationOtherEye = m_OtherEye.transform.localRotation;
        m_OtherEye.transform.localRotation *= m_LocalAdditionalGazeRotationOtherEye;
    }

    public override void LookAtGazeTarget(Vector3 targetLocation, bool previousGazeBonesReachedMaxAngle = true)
    {
        Vector3 directionThisFrame = GetTargetDirectionThisFrame(targetLocation);
        Vector3 directionThisFrameOtherEye = GetTargetDirectionThisFrameOtherEye(targetLocation);

        if(previousGazeBonesReachedMaxAngle)
        {
            RotateBone(directionThisFrame);
            RotateBoneOtherEye(directionThisFrameOtherEye);
        }
        else
        {
            if (IsDirectionCloserToBaseTransform(directionThisFrame) && IsDirectionCloserToBaseTransformOtherEye(directionThisFrameOtherEye))
            {
                RotateBone(directionThisFrame);
                RotateBoneOtherEye(directionThisFrameOtherEye);
            }
        }
    }

    public void RotateBoneOtherEye(Vector3 directionToFace)
    {
        m_OtherEye.transform.rotation = Quaternion.LookRotation(directionToFace);

        LimitEulerRotationOtherEye();

        m_LocalAdditionalGazeRotationOtherEye = Quaternion.Inverse(m_LocalAnimationRotationOtherEye) * m_OtherEye.transform.localRotation;
    }
    protected void LimitEulerRotationOtherEye()
    {
        Vector3 euler = m_OtherEye.transform.localEulerAngles;
        Vector3 eulerBase = m_BaseTransformObjectOtherEye.transform.localEulerAngles;


        if (euler.x > 180.0f) { euler.x -= 360.0f; }
        if (euler.y > 180.0f) { euler.y -= 360.0f; }
        if (euler.z > 180.0f) { euler.z -= 360.0f; }

        if (eulerBase.x > 180.0f) { eulerBase.x -= 360.0f; }
        if (eulerBase.y > 180.0f) { eulerBase.y -= 360.0f; }
        if (eulerBase.z > 180.0f) { eulerBase.z -= 360.0f; }

        Vector3 min = new Vector3(eulerBase.x + m_MinAngles.x, eulerBase.y - m_MaxAngles.y, eulerBase.z - m_MaxAngles.z);
        Vector3 max = new Vector3(eulerBase.x + m_MaxAngles.x, eulerBase.y - m_MinAngles.y, eulerBase.z - m_MinAngles.z);

        //Clamp X-Axis rotation
        m_ReachedMaxXAxisAngleOtherEye = false;
        if (euler.x < min.x)
        {
            euler.x = min.x;
            m_ReachedMaxXAxisAngleOtherEye = true;
        }
        else if (euler.x > max.x)
        {
            euler.x = max.x;
            m_ReachedMaxXAxisAngleOtherEye = true;
        }

        //Clamp Y-Axis rotation
        m_ReachedMaxYAxisAngleOtherEye = false;
        if (euler.y < min.y)
        {
            euler.y = min.y;
            m_ReachedMaxYAxisAngleOtherEye = true;
        }
        else if (euler.y > max.y)
        {
            euler.y = max.y;
            m_ReachedMaxYAxisAngleOtherEye = true;
        }

        //Clamp Z-Axis rotation
        m_ReachedMaxZAxisAngleOtherEye = false;
        if (euler.z < min.z)
        {
            euler.z = min.z;
            m_ReachedMaxZAxisAngleOtherEye = true;
        }
        else if (euler.z > max.z)
        {
            euler.z = max.z;
            m_ReachedMaxZAxisAngleOtherEye = true;
        }

        m_OtherEye.transform.localEulerAngles = euler;
    }


    public Vector3 GetOffsetTargetDirectionOtherEye(Vector3 targetLocation)
    {
        Vector3 offset = m_OtherEye.transform.right * -m_TargetOffset.x + m_OtherEye.transform.up * m_TargetOffset.y + m_OtherEye.transform.forward * m_TargetOffset.z;
        return targetLocation + offset - m_OtherEye.transform.position;
    }

    public Vector3 GetTargetDirectionThisFrameOtherEye(Vector3 targetLocation)
    {
        Vector3 possibleDirection = GetOffsetTargetDirectionOtherEye(targetLocation);

        if (Mathf.Abs(Vector3.SignedAngle(m_OtherEye.transform.forward, possibleDirection, m_OtherEye.transform.right)) > 90.0f)
        {
            float angleYAxis = Vector3.SignedAngle(m_OtherEye.transform.forward, possibleDirection, m_OtherEye.transform.up);
            possibleDirection = Quaternion.AngleAxis(angleYAxis, m_OtherEye.transform.up) * m_OtherEye.transform.forward;
        }

        float maxRadiansDelta = m_MaxTurnSpeed * Mathf.Deg2Rad * Time.deltaTime;
        return Vector3.RotateTowards(transform.forward, possibleDirection, maxRadiansDelta, 0.0f);
    }

    public bool IsDirectionCloserToBaseTransformOtherEye(Vector3 directionToTest)
    {
        float angleCurrent = Vector3.Angle(m_BaseTransformObjectOtherEye.transform.forward, m_OtherEye.transform.forward);
        float angleDirection = Vector3.Angle(m_BaseTransformObjectOtherEye.transform.forward, directionToTest);
        if (angleDirection < angleCurrent)
        {
            return true;
        }
        return false;
    }

    public override bool ReachedMaxXAxisAngle()
    {
        return m_ReachedMaxXAxisAngle || m_ReachedMaxXAxisAngleOtherEye;
    }

    public override bool ReachedMaxYAxisAngle()
    {
        return m_ReachedMaxYAxisAngle || m_ReachedMaxYAxisAngleOtherEye;
    }

    public override bool ReachedMaxZAxisAngle()
    {
        return m_ReachedMaxZAxisAngle || m_ReachedMaxZAxisAngleOtherEye;
    }

    public override Vector3 GetForward()
    {
        return (transform.forward + m_OtherEye.transform.forward).normalized;
    }
    public override Vector3 GetUp()
    {
        return (transform.up + m_OtherEye.transform.up).normalized;
    }
    public override Vector3 GetRight()
    {
        return (transform.right + m_OtherEye.transform.right).normalized;
    }
    public override Vector3 GetPosition()
    {
        return (transform.position + m_OtherEye.transform.position)*0.5f;
    }
}
