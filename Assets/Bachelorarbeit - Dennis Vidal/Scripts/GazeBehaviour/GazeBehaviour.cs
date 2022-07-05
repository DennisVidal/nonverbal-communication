using System.Collections.Generic;
using UnityEngine;

public class GazeBehaviour : MonoBehaviour
{   
    [Tooltip("All gaze behaviours that can be switched to from this gaze behaviour")]
    [SerializeField]
    protected List<GazeBehaviour> m_PossibleNextGazeBehaviours;

    protected Vector3 m_GazeTarget = Vector3.zero;
    protected bool m_ShouldChangeGazeBehaviour = false;
    protected bool m_CharacterArrivedAtGazeTarget = false;

    [Header("Cooldown")]
    [SerializeField]
    protected float m_MinCooldownTime = 0.0f;
    [SerializeField]
    protected float m_MaxCooldownTime = 0.0f;
    protected float m_CurrentMaxCooldownTime = 0.0f;
    protected float m_CurrentCooldownTimer = 0.0f;


    [Header("Gaze Time")]
    [SerializeField]
    protected float m_MaxGazeTimeMin = 0.0f;
    [SerializeField]
    protected float m_MaxGazeTimeMax = 0.0f;

    protected float m_CurrentGazeTime = 0.0f;
    protected float m_CurrentMaxGazeTime = 0.0f;

    [Range(0, 10)]
    [SerializeField]
    protected int m_GazeTimeProbabilityPower = 2;

    protected CharacterGaze m_CharacterGaze;

    protected virtual void Start()
    {
        m_CharacterGaze = GetComponent<CharacterGaze>();
        m_CurrentMaxGazeTime = Random.Range(m_MaxGazeTimeMin, m_MaxGazeTimeMax);
    }

    protected void Update()
    {
        if (IsOnCooldown())
        {
            m_CurrentCooldownTimer -= Time.deltaTime;
        }
    }

    public void UpdateBehaviour()
    {
        //Aktualisierung des Blickziels
        UpdateGazeTarget();
        //Hat der Charakter das Ziel erreicht?
        if (m_CharacterArrivedAtGazeTarget)
        {
            m_CurrentGazeTime += Time.deltaTime;
            //Kann der Charakter das Blickverhalten nicht mehr haben?
            if (!CanHaveBehaviour())
            {
                m_ShouldChangeGazeBehaviour = true;
            }
            else
            {
                //Sollte das Blickverhalten auf Grund der 
                //Wechselwahrhscheinlichkeit gewechselt werden?
                if (GetSwitchFromProbability() * Time.deltaTime > Random.value)
                {
                    m_ShouldChangeGazeBehaviour = true;
                }
            }
        }
        else
        {
            //Sieht der Charakter das Ziel an?
            if (m_CharacterGaze && m_CharacterGaze.IsLookingAtPosition(m_GazeTarget))
            {
                m_CharacterArrivedAtGazeTarget = true;
            }
        }
    }

    public virtual bool CanHaveBehaviour()
    {
        if (m_CurrentGazeTime > m_CurrentMaxGazeTime || IsOnCooldown())
        {
            return false;
        }

        return true;
    }

    public bool ShouldChangeBehaviour()
    {
        return m_ShouldChangeGazeBehaviour;
    }

    protected virtual void UpdateGazeTarget()
    {
    }

    public bool IsOnCooldown()
    {
        if (m_CurrentCooldownTimer > 0.0f)
        {
            return true;
        }
        return false;
    }

    public virtual float GetSwitchToProbability()
    {
        return 0.0f;
    }

    public virtual float GetSwitchFromProbability()
    {
        //Kann der Charakter das Blickverhalten nicht mehr haben?
        if (!CanHaveBehaviour())
        {
            return 1.0f;
        }

        float probability = 0.0f;
        if (m_PossibleNextGazeBehaviours.Count > 0)
        {
            for (int i = 0; i < m_PossibleNextGazeBehaviours.Count; i++)
            {
                probability += m_PossibleNextGazeBehaviours[i].GetSwitchToProbability();
            }

            probability /= m_PossibleNextGazeBehaviours.Count;
            probability *= GetGazeTimeSwitchProbability();
        }

        return probability;
    }

    public float GetGazeTimeSwitchProbability() 
    {
        float probability = Mathf.Clamp01(m_CurrentGazeTime / m_CurrentMaxGazeTime);
        probability = Mathf.Pow(probability, m_GazeTimeProbabilityPower);
        return probability;
    }

    public virtual void OnExitBehaviour(GazeBehaviour nextBehaviour = null)
    {
        m_ShouldChangeGazeBehaviour = false;
        m_CharacterArrivedAtGazeTarget = false;
        m_CurrentGazeTime = 0.0f;
        m_CurrentMaxGazeTime = Random.Range(m_MaxGazeTimeMin, m_MaxGazeTimeMax);
        m_CurrentCooldownTimer = m_CurrentMaxCooldownTime;
    }

    public virtual void OnEnterBehaviour(GazeBehaviour previousBehaviour = null)
    {
    }

    public Vector3 GetGazeTarget()
    {
        return m_GazeTarget;
    }

    public void SetGazeTarget(Vector3 position)
    {
        m_GazeTarget = position;
    }

    public List<GazeBehaviour> GetPossibleNextGazeBehaviours()
    {
        return m_PossibleNextGazeBehaviours;
    }
}