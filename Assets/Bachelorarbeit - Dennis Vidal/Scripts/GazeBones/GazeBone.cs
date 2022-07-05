using UnityEngine;


public class GazeBone : MonoBehaviour
{
   
    public virtual void UpdateGazeBoneRotation()
    {
    }

    public virtual void LookAtGazeTarget(Vector3 targetLocation, bool previousGazeBonesReachedMaxAngle = true)
    {
    }
    public bool ReachedMaxAngle(bool xAxis = true, bool yAxis = true, bool zAxis = true)
    {
        if(xAxis)
        {
            if(ReachedMaxXAxisAngle())
            {
                return true;
            }
        }

        if (yAxis)
        {
            if (ReachedMaxYAxisAngle())
            {
                return true;
            }
        }

        if (zAxis)
        {
            if (ReachedMaxZAxisAngle())
            {
                return true;
            }
        }

        return false;
    }
    public virtual bool ReachedMaxXAxisAngle()
    {
        return false;
    }
    public virtual bool ReachedMaxYAxisAngle()
    {
        return false;
    }
    public virtual bool ReachedMaxZAxisAngle()
    {
        return false;
    }

    public virtual Vector3 GetForward()
    {
        return transform.forward;
    }
    public virtual Vector3 GetUp()
    {
        return transform.up;
    }
    public virtual Vector3 GetRight()
    {
        return transform.right;
    }
    public virtual Vector3 GetPosition()
    {
        return transform.position;
    }
}

