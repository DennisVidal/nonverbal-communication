using UnityEngine;
using UnityEngine.AI;

public class AirTapAction : MonoBehaviour
{
    public GameObject m_CharacterToPlace;
    protected GameObject m_PlacedCharacter;

    public float m_MaxRaycastDistance = 10.0f;

    public float m_AirTapTimeNeededForPointing = 2.0f;
    protected float m_AirTapHeldTime = 0.0f;
    protected bool m_IsAirTapBeingHeld = false;


   
    void Update()
    {
        if (m_IsAirTapBeingHeld)
        {
            m_AirTapHeldTime += Time.deltaTime;
        }
    }

    public void OnPointerDown()
    {
        m_AirTapHeldTime = 0.0f;
        m_IsAirTapBeingHeld = true;
    }
    public void OnPointerUp()
    {
        m_IsAirTapBeingHeld = false;

        RaycastHit hit;
        Vector3 position = Vector3.zero;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, m_MaxRaycastDistance))
        {
            position = hit.point;
        }

        if (m_PlacedCharacter)
        {
            if (m_AirTapHeldTime >= m_AirTapTimeNeededForPointing)
            {
                CharacterGaze characterGaze = m_PlacedCharacter.GetComponent<CharacterGaze>();
                if (characterGaze)
                {
                    LookAtPointGazeBehaviour lookAtPointGazeBehaviour = m_PlacedCharacter.GetComponent<LookAtPointGazeBehaviour>();
                    if (lookAtPointGazeBehaviour)
                    {
                        characterGaze.SwitchToBehaviour(lookAtPointGazeBehaviour);
                        if(position == Vector3.zero)
                        {
                            position = Camera.main.transform.position + Camera.main.transform.forward * m_MaxRaycastDistance;
                        }
                        lookAtPointGazeBehaviour.SetGazeTarget(position);
                    }
                }
            }
            else
            {
                if (position != Vector3.zero)
                {
                    SetCharacterToPosition(position);
                    RotateCharacterToPosition(Camera.main.transform.position);
                }
            }
        }
        else
        {
            if (position != Vector3.zero)
            {
                m_PlacedCharacter = Instantiate(m_CharacterToPlace, position, new Quaternion());
                RotateCharacterToPosition(Camera.main.transform.position);
            }
        }
    }

    public void SetCharacterToPosition(Vector3 position)
    {
        if (m_PlacedCharacter)
        {
            m_PlacedCharacter.transform.position = position;
            NavMeshAgent agent = m_PlacedCharacter.GetComponent<NavMeshAgent>();
            if (agent)
            {
                agent.destination = m_PlacedCharacter.transform.position;
            }
        }
    }

    public void RotateCharacterToPosition(Vector3 position)
    {
        if (m_PlacedCharacter)
        {
            Vector3 direction = position - m_PlacedCharacter.transform.position;
            float angle = Vector3.SignedAngle(m_PlacedCharacter.transform.forward, direction, Vector3.up);
            m_PlacedCharacter.transform.rotation *= Quaternion.Euler(0.0f, angle, 0.0f);
        }
    }
}