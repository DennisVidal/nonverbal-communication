using UnityEngine;
public class Gaze : MonoBehaviour
{
    [Range(0.0f, 180.0f)]
    [SerializeField]
    protected float m_LookingAtAngle = 10.0f;

    [Range(0.0f, 100.0f)]
    [SerializeField]
    protected float m_MaxSeeDistance = 10.0f;


    public GameObject GetLookedAtObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(GetEyesPosition(), GetEyesForward(), out hit, m_MaxSeeDistance))
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    public bool IsLookingAtPosition(Vector3 position)
    {
        if (Vector3.Angle(GetEyesForward(), position - GetEyesPosition()) < m_LookingAtAngle)
        {
            return true;
        }

        return false;
    }

    public bool IsLookingAtObject(GameObject obj)
    {
        if(obj)
        {
            if (GetLookedAtObject() == obj)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsLookingAtObjectWithTag(string tag)
    {
        GameObject lookedAtObject = GetLookedAtObject();
        if(lookedAtObject)
        {
            if(lookedAtObject.CompareTag(tag))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsPositionInFOV(Vector3 position)
    {
        Vector3 directionEyesToPosition = position - GetEyesPosition();
        float verticalAngle = Vector3.SignedAngle(GetEyesForward(), directionEyesToPosition, GetEyesRight());
        if (Mathf.Abs(verticalAngle) <= (GetFOV(true) / 2.0f))
        {
            float horizontalAngle = Vector3.SignedAngle(GetEyesForward(), directionEyesToPosition, GetEyesUp());
            if (Mathf.Abs(horizontalAngle) <= (GetFOV(false) / 2.0f))
            {
                return true;
            }
        }
        return false;
    }

    public virtual Vector3 GetEyesForward()
    {
        return Vector3.zero;
    }

    public virtual Vector3 GetEyesUp()
    {
        return Vector3.zero;
    }

    public virtual Vector3 GetEyesRight()
    {
        return Vector3.zero;
    }

    public virtual Vector3 GetEyesPosition()
    {
        return Vector3.zero;
    }

    public virtual float GetFOV(bool vertical = false)
    {
        return 360.0f;
    }

    public virtual float ProbabilityOfSeeingGaze(Gaze gazeToSee)
    {
        return 1.0f;
    }
}
