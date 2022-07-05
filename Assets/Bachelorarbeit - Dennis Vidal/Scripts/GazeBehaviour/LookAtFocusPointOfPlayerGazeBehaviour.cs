using UnityEngine;

public class LookAtFocusPointOfPlayerGazeBehaviour : GazeBehaviour
{
    [Range(0.0f, 1.0f)]
    [Tooltip("The probability of switching to this behaviour, if the focus point of the target is recent")]
    [SerializeField]
    protected float m_FocusPointIsRecentProbability = 0.9f;

    [Range(0.0f, 1.0f)]
    [Tooltip("The probability of switching to this behaviour, if the focus point of the target is not recent")]
    [SerializeField]
    protected float m_FocusPointIsNotRecentProbability = 0.2f;

    protected PlayerGaze m_PlayerGaze;

    protected override void Start()
    {
        base.Start();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player)
        {
            m_PlayerGaze = player.GetComponent<PlayerGaze>();
        }

        if (m_PossibleNextGazeBehaviours.Count == 0)
        {
            m_PossibleNextGazeBehaviours.Add(GetComponent<LookAtPlayerGazeBehaviour>());
            m_PossibleNextGazeBehaviours.Add(GetComponent<WanderGazeBehaviour>());
        }
    }

    public override void OnEnterBehaviour(GazeBehaviour previousBehaviour = null)
    {
        base.OnEnterBehaviour(previousBehaviour);
        if (m_PlayerGaze.HasFocusPoint())
        {
            SetGazeTarget(m_PlayerGaze.GetFocusPoint().GetPosition());
        }
    }

    public override bool CanHaveBehaviour()
    {
        if (base.CanHaveBehaviour())
        {
            if (m_PlayerGaze)
            {
                if (m_PlayerGaze.HasFocusPoint())
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override float GetSwitchToProbability()
    {
        //Kann das Blickverhalten nicht eingenommen werden?
        if (!CanHaveBehaviour())
        {
            return 0.0f;
        }

        float probability = 0.0f;

        FocusPoint focusPoint = m_PlayerGaze.GetFocusPoint();
        if (focusPoint)
        {
            //Ist der fokussierte Punkt vor Kurzem erstellt worden?
            if (focusPoint.IsRecent())
            {
                probability = m_FocusPointIsRecentProbability;
            }
            else
            {
                probability = m_FocusPointIsNotRecentProbability;
            }
        }

        return probability;
    }
}
