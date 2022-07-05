using UnityEngine;

public class LookAtPlayerGazeBehaviour : GazeBehaviour
{
    [Tooltip("The base probability of looking at the player")]
    [Range(0.0f, 1.0f)]
    [SerializeField]
    protected float m_BaseProbability = 0.2f;

    protected PlayerGaze m_PlayerGaze;
    protected InterpersonalDistance m_InterpersonalDistance;

    protected override void Start()
    {
        base.Start();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player)
        {
            m_PlayerGaze = player.GetComponent<PlayerGaze>();
        }

        m_InterpersonalDistance = GetComponent<InterpersonalDistance>();

        if (m_PossibleNextGazeBehaviours.Count == 0)
        {
            m_PossibleNextGazeBehaviours.Add(GetComponent<LookAtFocusPointOfPlayerGazeBehaviour>());
            m_PossibleNextGazeBehaviours.Add(GetComponent<WanderGazeBehaviour>());
        }
    }

    protected override void UpdateGazeTarget()
    {
        if (m_PlayerGaze)
        {
            SetGazeTarget(m_PlayerGaze.GetEyesPosition());
        }
    }

    public override bool CanHaveBehaviour()
    {
        if (base.CanHaveBehaviour())
        {
            if (m_PlayerGaze)
            {
                return true;
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
        
        float probability = m_BaseProbability;
        //Sieht der Spieler den Charakter an?
        if (m_PlayerGaze.IsLookingAtObject(gameObject))
        {
            float seeingProbability = m_CharacterGaze.ProbabilityOfSeeingGaze(m_PlayerGaze);
            if (probability < seeingProbability)
            {
                probability = seeingProbability;
            }
        }

        float distanceToPlayer = m_InterpersonalDistance.GetHorizontalDistanceToTarget();
        float distanceProbability = 
            m_InterpersonalDistance.GetMutualGazeProbabilityAtDistance(distanceToPlayer);
        probability *= distanceProbability;

        return probability;
    }
}
