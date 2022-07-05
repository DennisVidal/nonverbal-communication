using UnityEngine;

public class LookAtPointGazeBehaviour : GazeBehaviour
{
    protected override void Start()
    {
        base.Start();

        if (m_PossibleNextGazeBehaviours.Count == 0)
        {
            m_PossibleNextGazeBehaviours.Add(GetComponent<LookAtPlayerGazeBehaviour>());
            m_PossibleNextGazeBehaviours.Add(GetComponent<WanderGazeBehaviour>());
        }
    }
}

