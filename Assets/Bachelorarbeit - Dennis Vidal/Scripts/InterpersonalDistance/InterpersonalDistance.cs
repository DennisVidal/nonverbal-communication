using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;

public class InterpersonalDistance : MonoBehaviour //Add Move as far as possible on direction in case there isn't a valid point to move to
{
    protected CharacterGaze m_CharacterGaze;
    protected Movement m_Movement;

    protected bool m_IsMovingIntoZone = false;

    [Tooltip("The target of this component. If set to null, the player will be selected")]
    [SerializeField]
    protected GameObject m_Target;

    [Tooltip("The different distance zones of the character")]
    [SerializeField]
    protected List<InterpersonalDistanceZone> m_InterpersonalDistanceZones;

    [Tooltip("The distance zone that the character currently tries to stay in")]
    [SerializeField]
    protected InterpersonalDistanceZone m_CurrentDistanceZone;

    [Tooltip("The horizontal distance at which the character would get touched by the target")]
    [SerializeField]
    protected float m_TouchDistance = 0.3f;

    [Tooltip("The distance the actual borders of the current distance zone get moved towards the center of the zone")]
    [SerializeField]
    protected float m_AdditionalZoneBorderDistance = 0.1f;

    [Tooltip("The amount of tries to find a position to move to per direction")]
    [Range(1, 30)]
    [SerializeField]
    protected int m_MaxSamplesPerDirection = 5;

    [Tooltip("The angle steps that the direction gets rotated by, if no suitable position could be found in that direction")]
    [Range(1, 60)]
    [SerializeField]
    protected int m_SampleStepAngle = 10;

    void Start()
    {
        m_CharacterGaze = GetComponent<CharacterGaze>();
        m_Movement = GetComponent<Movement>();

        if (!m_Target)
        {
            m_Target = GameObject.FindGameObjectWithTag("Player");
        }

        if(!m_CurrentDistanceZone && m_InterpersonalDistanceZones.Count > 0)
        {
            m_CurrentDistanceZone = m_InterpersonalDistanceZones[0];
        }
    }

    void Update()
    {
        if(m_IsMovingIntoZone)
        {
            if(m_Movement.ReachedDestination())
            {
                m_IsMovingIntoZone = false;
            }
        }
        
        if (ShouldMoveIntoCurrentZone())
        {
            MoveIntoCurrentZone();
        }

        if (IsTouchingTarget())
        {
            if(m_CharacterGaze)
            {
                if(m_CharacterGaze.GetCurrentGazeBehaviour())
                {
                    if (m_CharacterGaze.GetCurrentGazeBehaviour().GetType() != typeof(LookAtPlayerGazeBehaviour))
                    {
                        m_CharacterGaze.SwitchToLookAtPlayerBehaviour();
                    }
                }
            }
        }
    }

    public float GetHorizontalDistanceToTarget()
    {
        Vector3 targetPositionXZ = m_Target.transform.position;
        targetPositionXZ.y = 0.0f;
        Vector3 characterPositionXZ = transform.position;
        characterPositionXZ.y = 0.0f;

        return Vector3.Distance(targetPositionXZ, characterPositionXZ);
    }

    public bool IsTouchingTarget(float distanceToTarget = -1.0f)
    {
        float distance = distanceToTarget;
        if (distance == -1.0f)
        {
            distance = GetHorizontalDistanceToTarget();
        }

        if (distance < m_TouchDistance)
        {
            return true;
        }

        return false;
    }

    public bool IsTooCloseToTarget(float distanceToTarget = -1.0f)
    {
        float distance = distanceToTarget;
        if (distance == -1.0f)
        {
            distance = GetHorizontalDistanceToTarget(); 
        }

        if (distance < GetCurrentMinDistance())
        {
            return true;
        }

        return false;
    }

    public bool IsTooFarAwayFromTarget(float distanceToTarget = -1.0f)
    {
        float distance = distanceToTarget;
        if (distance == -1.0f)
        {
            distance = GetHorizontalDistanceToTarget();
        }

        if (distance > GetCurrentMaxDistance()) 
        {
            return true;
        }

        return false;
    }

    public bool ShouldMoveIntoCurrentZone()
    {
        float distanceToTarget = GetHorizontalDistanceToTarget();
        //Berührt der Spieler den Charakter?
        if (IsTouchingTarget(distanceToTarget))
        {
            return true;
        }
        //Sieht der Charakter den Spieler?
        if (m_CharacterGaze && m_CharacterGaze.IsPositionInFOV(m_Target.transform.position))
        {
            //Bewegt sich der Charakter bereits in die momentane Distanzzone?
            if (m_IsMovingIntoZone)
            {
                //Ist das Ziel der Bewegung nicht in der momentanen Distanzzone?
                if (!IsCurrentDestinationInCurrentZone())
                {
                    return true;
                }
            }
            else
            {
                //Ist der Charakter zu nah am Spieler?
                if (IsTooCloseToTarget(distanceToTarget))
                {
                    return true;
                }
                //Ist der Charakter zu weit entfernt vom Spieler?
                if (IsTooFarAwayFromTarget(distanceToTarget))
                {
                    //Sieht der Charakter den Spieler momentan an?
                    if (m_CharacterGaze.GetCurrentGazeBehaviour() &&
                        m_CharacterGaze.GetCurrentGazeBehaviour().GetType() == typeof(LookAtPlayerGazeBehaviour))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }


    public bool IsCurrentDestinationInCurrentZone()
    {
        return m_CurrentDistanceZone.IsPositionInZone(m_Target.transform.position, m_Movement.GetDestination(), m_TouchDistance);
    }

    public float GetCurrentMinDistance(float borderDistance = 0.0f)
    {
        float min = m_CurrentDistanceZone.GetMinDistance();
        float distance = m_TouchDistance + min;
        float zoneSize = m_CurrentDistanceZone.GetMaxDistance() - min;
        if (zoneSize < borderDistance)
        {
            distance += zoneSize * 0.5f;
        }
        else
        {
            distance += borderDistance;
        }

        return distance;
    }
    public float GetCurrentMaxDistance(float borderDistance = 0.0f)
    {
        float max = m_CurrentDistanceZone.GetMaxDistance();
        float distance = m_TouchDistance + max;
        float zoneSize = max - m_CurrentDistanceZone.GetMinDistance();
        if (zoneSize < borderDistance)
        {
            distance -= zoneSize * 0.5f;
        }
        else
        {
            distance -= borderDistance;
        }

        return distance;
    }

    public void MoveIntoCurrentZone()
    {
        Vector3 moveToPosition = GetMovePosition();
        if (moveToPosition != Vector3.zero)
        {
            m_Movement.SetMoveToPosition(moveToPosition);
            m_Movement.StartMoving();
            m_IsMovingIntoZone = true;
        }
        else if(m_IsMovingIntoZone)
        {
            m_Movement.StopMoving();
            m_IsMovingIntoZone = false;
        }
    }
   
    public Vector3 RotatePointHorizontally(Vector3 aroundPosition, Vector3 pointToRotate, float angle)
    {
        if(angle == 0.0f)
        {
            return pointToRotate;
        }

        return aroundPosition + RotateDirectionHorizontally(pointToRotate - aroundPosition, angle);
    }

    public Vector3 RotateDirectionHorizontally(Vector3 directionToRotate, float angle)
    {
        return Quaternion.AngleAxis(angle, Vector3.up) * directionToRotate;
    }

    public Vector3 GetMovePosition()
    {
        Vector3 targetPositionXZ = m_Target.transform.position;
        targetPositionXZ.y = transform.position.y;
        Vector3 directionFromTargetToCharacter = (transform.position - targetPositionXZ).normalized;

        float sampleRadius = m_Movement.GetStoppingDistance();

        float minZoneDistance = GetCurrentMinDistance(m_AdditionalZoneBorderDistance + sampleRadius);
        float maxZoneDistance = GetCurrentMaxDistance(m_AdditionalZoneBorderDistance + sampleRadius);

        Vector3 startPosition = targetPositionXZ + directionFromTargetToCharacter * minZoneDistance;

        float zoneSize = maxZoneDistance - minZoneDistance;

        Vector3 nextStartPosition, nextDirection, samplePosition;
        Vector3 foundPosition = Vector3.zero;
        Vector3 sampleUp = Vector3.up * m_Movement.GetNavMeshAgent().height;
        float sampleLength = m_Movement.GetNavMeshAgent().height * 2.0f;

        SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes surfaceType = SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes.Invalid;
        for (int i = 0; i <= 180; i+= m_SampleStepAngle)
        {
            nextStartPosition = RotatePointHorizontally(targetPositionXZ, startPosition, i);
            nextDirection = (nextStartPosition - targetPositionXZ).normalized;
            for (int k = 0; k < m_MaxSamplesPerDirection; k++)
            {
                samplePosition = nextStartPosition + nextDirection * Random.Range(0.0f, zoneSize);
                surfaceType = GameManager.Instance.GetSurfaceType(samplePosition + sampleUp, Vector3.down, sampleLength, out samplePosition);
                if (surfaceType == SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes.Floor || surfaceType == SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes.FloorLike)
                {
                    if (m_Movement.CanMoveToPosition(samplePosition, sampleRadius, out foundPosition))
                    {
                        return foundPosition;
                    }
                }
            }

            if(i != 0 && i != 180)
            {
                nextStartPosition = RotatePointHorizontally(targetPositionXZ, startPosition, -i);
                nextDirection = (nextStartPosition - targetPositionXZ).normalized;
                for (int k = 0; k < m_MaxSamplesPerDirection; k++)
                {
                    samplePosition = nextStartPosition + nextDirection * Random.Range(0.0f, zoneSize);
                    surfaceType = GameManager.Instance.GetSurfaceType(samplePosition + sampleUp, Vector3.down, sampleLength, out samplePosition);
                    if (surfaceType == SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes.Floor || surfaceType == SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes.FloorLike)
                    {
                        if (m_Movement.CanMoveToPosition(samplePosition, sampleRadius, out foundPosition))
                        {
                            return foundPosition;
                        }
                    }
                }
            }
        }
        return Vector3.zero;
    }
    public int GetIndexOfDistanceZoneByDistance(float distance)
    {
        float actualDistance = distance - m_TouchDistance;
        for (int i = 0; i < m_InterpersonalDistanceZones.Count; i++)
        {
            if (actualDistance < m_InterpersonalDistanceZones[i].GetMaxDistance())
            {
                if (actualDistance >= m_InterpersonalDistanceZones[i].GetMinDistance())
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public float GetMutualGazeProbabilityAtDistance(float distance)
    {
        float probability = 1.0f;
        if(m_InterpersonalDistanceZones.Count == 0)
        {
            return probability;
        }

        int index = GetIndexOfDistanceZoneByDistance(distance);
        if(index < 0)
        {
            return 1.0f;
        }

        float probabilityDistanceInZone = m_InterpersonalDistanceZones[index].GetMutualGazePobabilityDistance() + m_TouchDistance;
        float probabilityDistanceInNextZone;
        float lerpValue;

        if (distance < probabilityDistanceInZone)
        {
            if (index > 0)
            {
                probabilityDistanceInNextZone = m_InterpersonalDistanceZones[index - 1].GetMutualGazePobabilityDistance() + m_TouchDistance;
                lerpValue = (distance - probabilityDistanceInNextZone) / (probabilityDistanceInZone - probabilityDistanceInNextZone);
                probability = Mathf.Lerp(m_InterpersonalDistanceZones[index - 1].GetMutualGazePobability(), m_InterpersonalDistanceZones[index].GetMutualGazePobability(), lerpValue);
            }
            else
            {
                probability = m_InterpersonalDistanceZones[index].GetMutualGazePobability();
            }
        }
        else
        {
            if (index < m_InterpersonalDistanceZones.Count - 1)
            {
                probabilityDistanceInNextZone = m_InterpersonalDistanceZones[index + 1].GetMutualGazePobabilityDistance() + m_TouchDistance;
                lerpValue = (distance - probabilityDistanceInZone) / (probabilityDistanceInNextZone - probabilityDistanceInZone);
                probability = Mathf.Lerp(m_InterpersonalDistanceZones[index].GetMutualGazePobability(), m_InterpersonalDistanceZones[index + 1].GetMutualGazePobability(), lerpValue);
            }
            else
            {
                probability = m_InterpersonalDistanceZones[index].GetMutualGazePobability();
            }
        }
        return probability;
    }
}