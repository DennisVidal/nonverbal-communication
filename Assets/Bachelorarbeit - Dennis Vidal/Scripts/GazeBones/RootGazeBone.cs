using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootGazeBone : GazeBone
{
    protected Animator m_Animator;
    protected Movement m_Movement;

    [Tooltip("The Angle, between the forward direction of the bone and the target, that the character should start rotating at")]
    [SerializeField]
    protected float m_StartTurningAngle = 30.0f;

    protected void Start()
    {
        if (!m_Animator)
        {
            m_Animator = transform.root.gameObject.GetComponent<Animator>();
        }

        if (!m_Movement)
        {
            m_Movement = transform.root.gameObject.GetComponent<Movement>();
        }
    }

    public override void LookAtGazeTarget(Vector3 targetLocation, bool previousGazeBonesReachedMaxAngle = true)
    {
        //Haben alle vorherigen Knochen ihre maximalen Winkel erreicht?
        if (previousGazeBonesReachedMaxAngle)
        {
            //Dreht und bewegt sich der Charakter momentan nicht?
            if (!m_Movement.IsTurning() && !m_Movement.IsMoving())
            {
                Vector3 directionToTarget = targetLocation - GetPosition();

                float angle = Vector3.SignedAngle(GetForward(), directionToTarget, GetUp());
                //Ist der Winkel größer als der Winkel, ab dem die Rotation gestartet werden sollte?
                if (Mathf.Abs(angle) > m_StartTurningAngle)
                {
                    m_Movement.StartTurning(angle);
                }
            }
        }
        //Bewegt sich der Charakter?
        if (m_Movement.IsMoving())
        {
            m_Movement.TurnToPositionWhileMoving(targetLocation);
        }
    }
}
