using System.Collections.Generic;
using UnityEngine;

public class CharacterGaze : Gaze
{
    [Header("Gaze Behaviour")]
    [SerializeField]
    protected GazeBehaviour m_CurrentGazeBehaviour;

    [Header("Gaze Bones")]
    [SerializeField]
    protected List<GazeBone> m_GazeBones;

    [Range(0.0f, 180.0f)]
    [SerializeField]
    protected float m_InFocusAngle = 10.0f;

    [Tooltip("Total field of view of the character")]
    [Range(0.0f, 360.0f)]
    [SerializeField]
    protected float m_FOV = 180.0f;

    [Tooltip("Probability to see the target gaze while the target is in focus")]
    [Range(0.0f, 1.0f)]
    [SerializeField]
    protected float m_InFocusProbability = 1.0f;

    [Tooltip("The GazeBone that the character uses to see")]
    [SerializeField]
    protected GazeBone m_SeeingGazeBone;


    protected void Start()
    {
        if (!m_CurrentGazeBehaviour)
        {
            SwitchToBehaviour(GetComponent<LookAtPlayerGazeBehaviour>());
        }

        if (m_GazeBones.Count == 0)
        {
            RootGazeBone rootGazeBone = GetComponentInChildren<RootGazeBone>();
            if (rootGazeBone)
            {
                rootGazeBone.gameObject.GetComponentsInChildren<GazeBone>(m_GazeBones);
                m_GazeBones.Reverse();
            }
        }
        if (!m_SeeingGazeBone)
        {
            if (m_GazeBones.Count > 0)
            {
                m_SeeingGazeBone = m_GazeBones[0];
            }
        }

        GameManager.Instance.OnAirTappedOnCharacter += OnAirTappedOnCharacter;
        GameManager.Instance.OnStartedHoldingAirTap += OnStartedHoldingAirTap;
    }
    
    protected void Update()
    {
        m_CurrentGazeBehaviour.UpdateBehaviour();
        if (m_CurrentGazeBehaviour.ShouldChangeBehaviour())
        {
            SwitchToNextGazeBehaviour();
        }
    }
    protected void LateUpdate()
    {
        UpdateGazeBonesRotations();
        if(m_CurrentGazeBehaviour)
        {
            GazeBonesLookAtTarget(m_CurrentGazeBehaviour.GetGazeTarget());
        }
    }
    protected void UpdateGazeBonesRotations()
    {
        for (int k = 0; k < m_GazeBones.Count; k++)
        {
            m_GazeBones[k].UpdateGazeBoneRotation();
        }
    }
    protected void GazeBonesLookAtTarget(Vector3 targetPosition)
    {
        bool previousBonesReachedMaxAngle = true;
        for (int k = 0; k < m_GazeBones.Count; k++)
        {
            m_GazeBones[k].LookAtGazeTarget(targetPosition, previousBonesReachedMaxAngle);
            //Hat der Knochen den maximalen Winkel weder entlang der X- noch der Y-Achse erreicht?
            if (!m_GazeBones[k].ReachedMaxAngle(true, true, false))
            {
                previousBonesReachedMaxAngle = false;
            }
        }
    }

    public GazeBehaviour GetCurrentGazeBehaviour()
    {
        return m_CurrentGazeBehaviour;
    }

    protected void SwitchToNextGazeBehaviour()
    {
        List<float> probabilities = new List<float>();
        float sumOfProbabilities = 0.0f;
        List<GazeBehaviour> possibleGazeBehaviours = m_CurrentGazeBehaviour.GetPossibleNextGazeBehaviours();
        for (int i = 0; i < possibleGazeBehaviours.Count; i++)
        {
            probabilities.Add(possibleGazeBehaviours[i].GetSwitchToProbability());
            sumOfProbabilities += probabilities[probabilities.Count - 1];
        }

        float randomValue = Random.Range(0.0f, sumOfProbabilities);
        float currentProbability = 0.0f;
        for (int k = 0; k < probabilities.Count; k++)
        {
            currentProbability += probabilities[k];
            if (currentProbability > randomValue)
            {
                SwitchToBehaviour(possibleGazeBehaviours[k]);
                break;
            }
        }
    }

    public void SwitchToBehaviour(GazeBehaviour behaviourToSwitchTo)
    {
        if (m_CurrentGazeBehaviour)
        {
            m_CurrentGazeBehaviour.OnExitBehaviour(behaviourToSwitchTo);
        }

        if (behaviourToSwitchTo)
        {
            behaviourToSwitchTo.OnEnterBehaviour(m_CurrentGazeBehaviour);
        }

        m_CurrentGazeBehaviour = behaviourToSwitchTo;
    }
   
    public override Vector3 GetEyesForward()
    {
        if (m_SeeingGazeBone)
        {
            return m_SeeingGazeBone.GetForward();
        }
        else
        {
            return transform.forward;
        }
    }

    public override Vector3 GetEyesUp()
    {
        if (m_SeeingGazeBone)
        {
            return m_SeeingGazeBone.GetUp();
        }
        else
        {
            return transform.up;
        }
    }

    public override Vector3 GetEyesRight()
    {
        if (m_SeeingGazeBone)
        {
            return m_SeeingGazeBone.GetRight();
        }
        else
        {
            return transform.right;
        }
    }

    public override Vector3 GetEyesPosition()
    {
        if (m_SeeingGazeBone)
        {
            return m_SeeingGazeBone.GetPosition();
        }
        else
        {
            return transform.position;
        }
    }

    public override float GetFOV(bool vertical = false)
    {
        return m_FOV;
    }

    public override float ProbabilityOfSeeingGaze(Gaze gazeToSee)
    {
        float probability = 0.0f;
        if (gazeToSee)
        {
            Vector3 directionToTargetEyes = gazeToSee.GetEyesPosition() - GetEyesPosition();
            float angleToTarget = Vector3.Angle(GetEyesForward(), directionToTargetEyes);
            //Ist der Winkel zum Ziel kleiner als der Fokuswinkel?
            if (angleToTarget < m_InFocusAngle)
            {
                probability = m_InFocusProbability;
            }
            else
            {
                float halfFOV = GetFOV() / 2.0f;
                probability = 1.0f - ((angleToTarget - m_InFocusAngle) / (halfFOV - m_InFocusAngle));
                probability = Mathf.Clamp01(probability);
                //Ist die Wahrscheinlichkeit größer als die Wahrscheinlichkeit im Fokus?
                if (probability > m_InFocusProbability)
                {
                    probability = m_InFocusProbability;
                }
            }
        }

        return probability;
    }

    public float GetSumOfProbabilities(List<float> floatList, int fromIndex = 0, int toIndex = -1)
    {
        if (toIndex == -1)
        {
            toIndex = floatList.Count - 1;
        }

        float SumOfFloats = 0.0f;
        for (int i = fromIndex; i <= toIndex; i++)
        {
            SumOfFloats += floatList[i];
        }
        return SumOfFloats;
    }

    public void SwitchToLookAtPointBehaviour(Vector3 targetPosition)
    {
        LookAtPointGazeBehaviour lookAtPointGazeBehaviour = GetComponent<LookAtPointGazeBehaviour>();
        if (lookAtPointGazeBehaviour)
        {
            SwitchToBehaviour(lookAtPointGazeBehaviour);
            lookAtPointGazeBehaviour.SetGazeTarget(targetPosition);
        }
    }

    public void SwitchToLookAtPlayerBehaviour()
    {
        LookAtPlayerGazeBehaviour lookAtPlayerGazeBehaviour = GetComponent<LookAtPlayerGazeBehaviour>();
        if (lookAtPlayerGazeBehaviour)
        {
            SwitchToBehaviour(lookAtPlayerGazeBehaviour);
        }
    }

    protected void OnAirTappedOnCharacter()
    {
        SwitchToLookAtPlayerBehaviour();
    }

    protected void OnStartedHoldingAirTap(Vector3 position)
    {
        SwitchToLookAtPointBehaviour(position);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnAirTappedOnCharacter -= OnAirTappedOnCharacter;
        GameManager.Instance.OnStartedHoldingAirTap -= OnStartedHoldingAirTap;
    }
}