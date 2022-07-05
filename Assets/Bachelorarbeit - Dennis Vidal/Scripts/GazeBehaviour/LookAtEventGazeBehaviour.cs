using System.Collections.Generic;
using UnityEngine;

public class LookAtEventGazeBehaviour : GazeBehaviour
{
    [Tooltip("The base probability of noticing any event. This gets multiplied with the look at probability of an event")]
    [SerializeField]
    protected float m_NoticeEventProbability = 1.0f;

    protected List<LookAtEvent> m_LookAtEvents;
    protected LookAtEvent m_CurrentLookAtEvent;

    protected override void Start()
    {
        base.Start();

        m_LookAtEvents = new List<LookAtEvent>();
        GameManager.Instance.OnLookAtEventRegistered += OnLookAtEventRegistered;
        GameManager.Instance.OnLookAtEventUnregistered += OnLookAtEventUnregistered;

        if (m_PossibleNextGazeBehaviours.Count == 0)
        {
            m_PossibleNextGazeBehaviours.Add(GetComponent<LookAtPlayerGazeBehaviour>());
            m_PossibleNextGazeBehaviours.Add(GetComponent<WanderGazeBehaviour>());
        }
    }

    protected override void UpdateGazeTarget()
    {
        if (m_CurrentLookAtEvent)
        {
            m_GazeTarget = m_CurrentLookAtEvent.GetPosition();
        }
    }

    protected void OnLookAtEventRegistered(LookAtEvent lookAtEvent)
    {
        m_LookAtEvents.Add(lookAtEvent);
        lookAtEvent.OnLookAtEvent += OnLookAtEvent;
    }
    protected void OnLookAtEventUnregistered(LookAtEvent lookAtEvent)
    {
        m_LookAtEvents.Remove(lookAtEvent);
        lookAtEvent.OnLookAtEvent -= OnLookAtEvent;
    }

    protected void OnLookAtEvent(LookAtEvent lookAtEvent)
    {
        if (lookAtEvent)
        {
            if (lookAtEvent.GetLookAtEventProbability() * m_NoticeEventProbability > Random.value)
            {
                m_CurrentLookAtEvent = lookAtEvent;
                if (m_CharacterGaze)
                {
                    m_CharacterGaze.SwitchToBehaviour(this); //It would probably be better to have the behaviour switch in CharacterGaze instead of here
                }
            }
        }
    }


    void OnDestroy()
    {
        GameManager.Instance.OnLookAtEventRegistered -= OnLookAtEventRegistered;
        GameManager.Instance.OnLookAtEventUnregistered -= OnLookAtEventUnregistered;
        for (int i = 0; i < m_LookAtEvents.Count; i++)
        {
            if(m_LookAtEvents[i])
            {
                m_LookAtEvents[i].OnLookAtEvent -= OnLookAtEvent;
            }
        }
    }
}
