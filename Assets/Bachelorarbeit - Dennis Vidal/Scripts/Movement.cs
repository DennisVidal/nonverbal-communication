using UnityEngine.AI;
using HoloToolkit.Unity;
using UnityEngine;

public class Movement : MonoBehaviour
{
    protected NavMeshAgent m_NavMeshAgent;
    protected Animator m_Animator;


    [SerializeField]
    protected AudioSource m_AudioSource;
    [SerializeField]
    protected AudioClip m_StepSound;

    void Start()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();

        m_NavMeshAgent.destination = transform.position;
        m_NavMeshAgent.updatePosition = false;
        m_NavMeshAgent.updateRotation = false;
    }

    void Update()
    {
        if (IsMoving())
        {
            //Needed to stop keep the character grounded in certain situations
            Vector3 hitPosition = Vector3.zero;
            SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes surfaceType = GameManager.Instance.GetSurfaceType(transform.position, Vector3.down, 0.5f, out hitPosition);
            if(hitPosition != Vector3.zero)
            {
                if (surfaceType == SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes.Floor || surfaceType == SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes.FloorLike)
                {
                    transform.position = new Vector3(transform.position.x, hitPosition.y, transform.position.z);
                }
            }
            //Stop movement when the destination is reached
            if (ReachedDestination())
            {
                StopMoving();
            }
        }

        m_NavMeshAgent.nextPosition = transform.position;

        Vector3 directionToDestination = transform.InverseTransformDirection(m_NavMeshAgent.steeringTarget - transform.position).normalized * m_NavMeshAgent.speed;

        m_Animator.SetFloat("VelX", directionToDestination.x);
        m_Animator.SetFloat("VelZ", directionToDestination.z);
    }

    public void SetMoveToPosition(Vector3 position)
    {
        NavMeshPath path = new NavMeshPath();
        m_NavMeshAgent.CalculatePath(position, path);
        if(path.status == NavMeshPathStatus.PathComplete)
        {
            m_NavMeshAgent.SetDestination(position);
            //m_NavMeshAgent.SetPath(path);
        }
        else
        {
            m_NavMeshAgent.SetDestination(transform.position);
        }
    }

    public void StartMoving()
    {
        if(!ReachedDestination())
        {
            m_Animator.SetBool("IsMoving", true);
        }
    }

    public void StopMoving()
    {
        m_Animator.SetBool("IsMoving", false);
        m_NavMeshAgent.SetDestination(transform.position);
    }
    public bool IsMoving()
    {
        return m_Animator.GetBool("IsMoving");
    }

    public bool ReachedDestination()
    {
        if (!m_NavMeshAgent.pathPending)
        {
            if (m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance)
            {
                if (!m_NavMeshAgent.hasPath || m_NavMeshAgent.velocity.sqrMagnitude == 0.0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public float GetStoppingDistance()
    {
        return m_NavMeshAgent.stoppingDistance;
    }

    public Vector3 GetDestination()
    {
        return m_NavMeshAgent.destination;
    }

    public void StartTurning(float angleToTurn)
    {
        float blendValue = angleToTurn / 90.0f;
        m_Animator.SetFloat("TurningBlendValue", blendValue);
        m_Animator.SetBool("IsTurning", true);
    }

    public void StopTurning()
    {
        m_Animator.SetBool("IsTurning", false);
        m_Animator.SetFloat("TurningBlendValue", 0.0f);
    }
    public bool IsTurning()
    {
        return m_Animator.GetBool("IsTurning");
    }

    public void TurnToPositionWhileMoving(Vector3 position)
    {
        if(IsMoving())
        {
            Vector3 directionThisTick = Vector3.RotateTowards(transform.forward, position - transform.position, m_NavMeshAgent.angularSpeed * Mathf.Deg2Rad * Time.deltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(directionThisTick);
        }
    }
    public bool CanMoveToPosition(Vector3 positionToSampleAt, float sampleRadius, out Vector3 foundPosition)
    {
        NavMeshHit hit;
        NavMesh.SamplePosition(positionToSampleAt, out hit, sampleRadius, 1);
        if (hit.hit)
        {
            NavMeshPath path = new NavMeshPath();
            m_NavMeshAgent.CalculatePath(hit.position, path);
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                foundPosition = hit.position;
                return true;
            }
        }
        foundPosition = Vector3.zero;
        return false;

    }

    public NavMeshAgent GetNavMeshAgent()
    {
        return m_NavMeshAgent;
    }

    public void PlayStepSound()
    {
        if(m_AudioSource)
        {
            if (m_StepSound)
            {
                m_AudioSource.clip = m_StepSound;
                m_AudioSource.Play();
            }
        }
    }
}
