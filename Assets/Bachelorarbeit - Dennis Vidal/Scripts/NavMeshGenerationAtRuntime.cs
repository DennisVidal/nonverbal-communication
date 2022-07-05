using UnityEngine.AI;
using UnityEngine;


public class NavMeshGenerationAtRuntime : MonoBehaviour
{

    NavMeshSurface m_NavMeshSurface;

    protected bool m_IsNavMeshBuilt = false;
    void Start()
    {
        m_NavMeshSurface = GetComponent<NavMeshSurface>();
        GameManager.Instance.OnFinishedRoomSetup += OnFinishedRoomSetup;
    }

    void OnFinishedRoomSetup()
    {
        if (m_NavMeshSurface)
        {
            m_NavMeshSurface.BuildNavMesh();
            m_IsNavMeshBuilt = true;
        }
    }

    public bool IsNavMeshBuilt()
    {
        return m_IsNavMeshBuilt;
    }

    void OnDestroy()
    {
        GameManager.Instance.OnFinishedRoomSetup -= OnFinishedRoomSetup;
    }
}
