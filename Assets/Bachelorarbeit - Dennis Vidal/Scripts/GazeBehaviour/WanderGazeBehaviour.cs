using System.Collections.Generic;
using UnityEngine;

public class WanderGazeBehaviour : GazeBehaviour
{

    [Tooltip("The max movement speed of the gaze target per second")]
    [SerializeField]
    protected float m_MaxSpeed = 0.2f;


    [Tooltip("The max direction change per second")]
    [SerializeField]
    protected Vector2 m_MaxMultiplierChanges = Vector2.one;

    [Tooltip("The max direction that the character can look up")]
    [SerializeField]
    protected float m_MaxUp = 0.3f;

    [Tooltip("The max direction that the character can look down")]
    [SerializeField]
    protected float m_MaxDown = 0.3f;

    protected Vector2 m_DirectionMultiplier;

    protected override void Start()
    {
        base.Start();

        m_GazeTarget = m_CharacterGaze.GetEyesPosition() + m_CharacterGaze.GetEyesForward();

        if (m_PossibleNextGazeBehaviours.Count == 0)
        {
            m_PossibleNextGazeBehaviours.Add(GetComponent<LookAtPlayerGazeBehaviour>());
        }
    }

    protected override void UpdateGazeTarget()
    {
        Vector2 multiplierChangesThisFrame = new Vector2(m_MaxMultiplierChanges.x * Time.deltaTime,
                                                         m_MaxMultiplierChanges.y * Time.deltaTime);
        m_DirectionMultiplier.x += Random.Range(-multiplierChangesThisFrame.x, multiplierChangesThisFrame.x);
        m_DirectionMultiplier.x = Mathf.Clamp(m_DirectionMultiplier.x, -1.0f, 1.0f);
        m_DirectionMultiplier.y += Random.Range(-multiplierChangesThisFrame.y, multiplierChangesThisFrame.y);
        m_DirectionMultiplier.y = Mathf.Clamp(m_DirectionMultiplier.y, -1.0f, 1.0f);

        float speedThisFrame = m_MaxSpeed * Time.deltaTime;
        Vector3 horizontalStep = m_CharacterGaze.GetEyesRight() * m_DirectionMultiplier.x * speedThisFrame;
        Vector3 verticalStep = m_CharacterGaze.GetEyesUp() * m_DirectionMultiplier.y * speedThisFrame;

        Vector3 eyePosition = m_CharacterGaze.GetEyesPosition();
        Vector3 newTargetPosition = m_GazeTarget + horizontalStep + verticalStep;
        m_GazeTarget = eyePosition + (newTargetPosition - eyePosition).normalized;

        Vector3 forward = transform.forward, right = transform.right;
        Vector3 directionTarget = m_GazeTarget - transform.position;
        Vector3 directionEyes = eyePosition - transform.position;
        Vector3 projectedTarget = Vector3.ProjectOnPlane(Vector3.ProjectOnPlane(directionTarget, forward), right);
        Vector3 projectedEyes = Vector3.ProjectOnPlane(Vector3.ProjectOnPlane(directionEyes, forward), right);
        float eyeToTargetDistanceDelta = projectedTarget.magnitude - projectedEyes.magnitude;
        if (eyeToTargetDistanceDelta > m_MaxUp)
        {
            m_DirectionMultiplier.y -= multiplierChangesThisFrame.y;
            m_DirectionMultiplier.y = Mathf.Clamp(m_DirectionMultiplier.y, -1.0f, 1.0f);
        }
        else if (eyeToTargetDistanceDelta < -m_MaxDown)
        {
            m_DirectionMultiplier.y += multiplierChangesThisFrame.y;
            m_DirectionMultiplier.y = Mathf.Clamp(m_DirectionMultiplier.y, -1.0f, 1.0f);
        }
    }

    public override float GetSwitchToProbability()
    {
        //Kann das Blickverhalten nicht eingenommen werden?
        if (!CanHaveBehaviour())
        {
            return 0.0f;
        }

        float probability = 0.0f;
        List<GazeBehaviour> possibleGazeBehaviours = 
            m_CharacterGaze.GetCurrentGazeBehaviour().GetPossibleNextGazeBehaviours();
        int behaviourCount = possibleGazeBehaviours.Count;
        for (int i=0; i < possibleGazeBehaviours.Count; i++)
        {
            //Ist das Blickverhalten kein Wander-Blickverhalten?
            if(possibleGazeBehaviours[i].GetType() != GetType())
            {
                probability += 1.0f - possibleGazeBehaviours[i].GetSwitchToProbability();
            }
            else
            {
                behaviourCount--;
            }
        }
        //Ist der Wechsel nicht nur zu Wander-Blickverhalten möglich?
        if(behaviourCount > 0)
        {
            probability /= behaviourCount;
        }
        else
        {
            probability = 1.0f;
        }

        return probability;
    }

    public override void OnExitBehaviour(GazeBehaviour nextBehaviour = null)
    {
        base.OnExitBehaviour(nextBehaviour);
        m_DirectionMultiplier = Vector2.zero;
    }

    public override void OnEnterBehaviour(GazeBehaviour previousBehaviour = null)
    {
        base.OnEnterBehaviour(previousBehaviour);
        if (previousBehaviour)
        {
            SetGazeTarget(previousBehaviour.GetGazeTarget());
        }
    }
}