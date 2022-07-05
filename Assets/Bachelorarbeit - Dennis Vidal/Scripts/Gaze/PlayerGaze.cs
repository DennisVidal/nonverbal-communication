using System.Collections.Generic;
using UnityEngine;

public class PlayerGaze : Gaze
{
    protected FocusPoint m_FocusPoint;

    [SerializeField]
    protected List<Vector3> m_LookAtPoints = new List<Vector3>();
    [SerializeField]
    protected int m_MaxLookAtPoints = 20;

    [Header("Look At Points")]
    [SerializeField]
    protected int m_LookAtPointsNeededForFocusPoint = 4;
    [SerializeField]
    protected float m_MaxDistanceBetweenLookAtPointsForFocusPoint = 0.5f;

    [SerializeField]
    protected float m_TimeBetweenLookAtPointCreationMin = 0.0f;
    [SerializeField]
    protected float m_TimeBetweenLookAtPointCreationMax = 0.5f;
    protected float m_LookAtPointCreationTimer = 0.0f;


    protected void Start()
    {
        m_FocusPoint = GetComponent<FocusPoint>();
        m_LookAtPointCreationTimer = Random.Range(m_TimeBetweenLookAtPointCreationMin, m_TimeBetweenLookAtPointCreationMax);
    }

    void Update()
    {
        m_LookAtPointCreationTimer -= Time.deltaTime;
        if (m_LookAtPointCreationTimer < 0.0f)
        {
            if (!IsLookingAtObjectWithTag("Character"))
            {
                Vector3 position = GetLookedAtPosition();
                UpdateFocusPoint(position);
                AddLookAtPoint(position);
            }

            m_LookAtPointCreationTimer = Random.Range(m_TimeBetweenLookAtPointCreationMin, m_TimeBetweenLookAtPointCreationMax);
        }
    }
    public void UpdateFocusPoint(Vector3 position)
    {
        int closeEnoughCount = 0;
        for (int i = 0; i < m_LookAtPoints.Count; i++)
        {
            if (Vector3.Distance(m_LookAtPoints[i], position) < m_MaxDistanceBetweenLookAtPointsForFocusPoint)
            {
                closeEnoughCount++;
            }
        }

        if (closeEnoughCount >= m_LookAtPointsNeededForFocusPoint)
        {
            m_FocusPoint.SetPosition(position);
        }
    }

    public Vector3 GetLookedAtPosition()
    {
        RaycastHit hit;
        if (Physics.Raycast(GetEyesPosition(), GetEyesForward(), out hit, m_MaxSeeDistance))
        {
            return hit.point;
        }
        else
        {
            return GetEyesPosition() + GetEyesForward() * m_MaxSeeDistance;
        }
    }

    public void AddLookAtPoint(Vector3 position)
    {
        m_LookAtPoints.Add(position);

        if (m_LookAtPoints.Count > m_MaxLookAtPoints)
        {
            while (m_LookAtPoints.Count > m_MaxLookAtPoints)
            {
                m_LookAtPoints.RemoveAt(0);
            }
        }
    }

    public override Vector3 GetEyesForward()
    {
        return Camera.main.transform.forward;
    }

    public override Vector3 GetEyesUp()
    {
        return Camera.main.transform.up;
    }

    public override Vector3 GetEyesRight()
    {
        return Camera.main.transform.right;
    }

    public override Vector3 GetEyesPosition()
    {
        return Camera.main.transform.position;
    }

    public override float GetFOV(bool vertical = false)
    {
        if (vertical)
        {
            return Camera.main.fieldOfView;
        }
        else
        {
            float radianVerticalFOV = Camera.main.fieldOfView * Mathf.Deg2Rad;
            float radianHorizontalFOV = 2 * Mathf.Atan(Mathf.Tan(radianVerticalFOV / 2) * Camera.main.aspect);
            return Mathf.Rad2Deg * radianHorizontalFOV;
        }
    }

    public override float ProbabilityOfSeeingGaze(Gaze gazeToSee)
    {
        if (gazeToSee)
        {
            if(IsPositionInFOV(gazeToSee.GetEyesPosition()))
            {
                return 1.0f;
            }
        }

        return 0.0f;
    }

    public bool HasFocusPoint()
    {
        if (m_FocusPoint)
        {
            if (!m_FocusPoint.IsTooOld())
            {
                return true;
            }
        }

        return false;
    }

    public FocusPoint GetFocusPoint()
    {
        return m_FocusPoint;
    }
}