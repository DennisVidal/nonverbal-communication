using System;
using System.Collections.Generic;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine;
using UnityEngine.AI;


public class GameManager : MonoBehaviour, IInputClickHandler, IHoldHandler
{
    public static GameManager Instance = null;

    [SerializeField]
    GameObject m_CharacterToPlace;
    GameObject m_PlacedCharacter;

    [SerializeField]
    bool m_DisableMeshOnScanDone = true;

    [SerializeField]
    float m_MaxRaycastDistance = 10.0f;

    [SerializeField]
    float m_MinHorizontalAreaNeeded = 20.0f;
    float m_ScannedHorizontalArea = 0.0f;

    [SerializeField]
    Transform m_CameraTransform;

    NavMeshGenerationAtRuntime m_NavMeshGenerationAtRuntime;

    [SerializeField]
    float m_RoomSetupDelayTime = 0.3f; //Might not be needed anymore
    float m_RoomSetupDelayTimer = -1.0f;

    bool m_IsAirTapBeingHeld = false;
    float m_IsAirTapBeingHeldTime = 0.0f;
    float m_TimeNeededForSettingOfPosition = 1.0f;


    List<LookAtEvent> m_LookAtEvents = new List<LookAtEvent>();

    public event Action OnStartedScanningRoom;
    public event Action OnScannedMinArea;
    // public event Action OnFinishedScanningRoom;
    public event Action OnFinishedRoomSetup;
    public event Action OnPlacedCharacter;

    public event Action<Vector3> OnAirTapped;
    public event Action<Vector3> OnStartedHoldingAirTap;
    public event Action<Vector3> OnStoppedHoldingAirTap;

    public event Action OnAirTappedOnCharacter;
    public event Action<Vector3> OnAirTappedNotOnCharacter;

    public event Action<LookAtEvent> OnLookAtEventRegistered;
    public event Action<LookAtEvent> OnLookAtEventUnregistered;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        m_NavMeshGenerationAtRuntime = GetComponent<NavMeshGenerationAtRuntime>();

        m_CameraTransform = Camera.main.transform;

        SpatialUnderstanding.Instance.ScanStateChanged += OnScanStateChanged;
        // SurfaceMeshesToPlanes.Instance.MakePlanesComplete += OnMakePlanesComplete;
    }

    void Update()
    {
        if (!HasScannedMinHorizontalArea())
        {
            m_ScannedHorizontalArea = GetScannedHorizontalArea();
            if (HasScannedMinHorizontalArea())
            {
                if (OnScannedMinArea != null)
                {
                    OnScannedMinArea();
                }
            }
        }


        if (m_RoomSetupDelayTimer > 0.0f)
        {
            m_RoomSetupDelayTimer -= Time.deltaTime;
            if (m_RoomSetupDelayTimer < 0.0f)
            {
                m_RoomSetupDelayTimer = -1.0f;

                if (OnFinishedRoomSetup != null)
                {
                    OnFinishedRoomSetup();
                }
            }
        }

        if(m_IsAirTapBeingHeld)
        {
            m_IsAirTapBeingHeldTime += Time.deltaTime;
        }
    }

    public void OnScanStateChanged()
    {
        switch (SpatialUnderstanding.Instance.ScanState)
        {
            case SpatialUnderstanding.ScanStates.ReadyToScan:
                SpatialUnderstanding.Instance.RequestBeginScanning();
                break;
            case SpatialUnderstanding.ScanStates.Scanning:
                DisableSpatialUnderstandingMeshWire(false);
                if (OnStartedScanningRoom != null)
                {
                    OnStartedScanningRoom();
                }
                break;
            case SpatialUnderstanding.ScanStates.Done:
                m_RoomSetupDelayTimer = m_RoomSetupDelayTime; //
                // CreatePlanes();
                //  if(OnFinishedScanningRoom != null) 
                //  {
                //      OnFinishedScanningRoom();
                //   }
                break;
            case SpatialUnderstanding.ScanStates.None:
            case SpatialUnderstanding.ScanStates.Finishing:
            default:
                break;
        }
    }
    void DisableSpatialUnderstandingMeshWire(bool disableMesh = false)
    {
        if (disableMesh)
        {
            SpatialUnderstanding.Instance.UnderstandingCustomMesh.MeshMaterial.SetColor("_WireColor", Color.black); 
        }
        else
        {
            SpatialUnderstanding.Instance.UnderstandingCustomMesh.MeshMaterial.SetColor("_WireColor", Color.white);
        }
    }

    public SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes GetSurfaceType(Vector3 rayOrigin, Vector3 rayDirection, float rayLength, out Vector3 hitPosition)
    {
        if (SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
        {
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
            {
                Vector3 raycastOrigin = rayOrigin;
                Vector3 raycastDirection = rayDirection.normalized * rayLength;
                IntPtr raycastResultPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticRaycastResultPtr();
                SpatialUnderstandingDll.Imports.PlayspaceRaycast(raycastOrigin.x, raycastOrigin.y, raycastOrigin.z, raycastDirection.x, raycastDirection.y, raycastDirection.z, raycastResultPtr);
                hitPosition = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticRaycastResult().IntersectPoint;
                return SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticRaycastResult().SurfaceType;
            }
        }
        hitPosition = Vector3.zero;
        return SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes.Invalid;
    }
    public bool HasScannedMinHorizontalArea()
    {
        if (m_ScannedHorizontalArea > m_MinHorizontalAreaNeeded)
        {
            return true;
        }
        return false;
    }
    public float GetMinHorizontalAreaNeeded()
    {
        return m_MinHorizontalAreaNeeded;
    }

    public float GetScannedHorizontalArea()
    {
        if (SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
        {
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning)
            {
                IntPtr playspaceStatsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(playspaceStatsPtr) != 0)
                {
                    SpatialUnderstandingDll.Imports.PlayspaceStats playspaceStats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();
                    return playspaceStats.HorizSurfaceArea;
                }
            }
        }
        return -1.0f;
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        RaycastHit hit;
        Vector3 position = Vector3.zero;
        if (Physics.Raycast(m_CameraTransform.position, m_CameraTransform.forward, out hit, m_MaxRaycastDistance))
        {
            position = hit.point;
            if (hit.collider.tag == "Character")
            {
                if (OnAirTappedOnCharacter != null)
                {
                    OnAirTappedOnCharacter();
                }
            }
            else
            {
                if (OnAirTappedNotOnCharacter != null)
                {
                    OnAirTappedNotOnCharacter(position);
                }
            }
        }
        else
        {
            if (OnAirTappedNotOnCharacter != null)
            {
                OnAirTappedNotOnCharacter(m_CameraTransform.position + m_CameraTransform.forward * m_MaxRaycastDistance);
            }
        }
        if (OnAirTapped != null)
        {
            OnAirTapped(position);
        }

        if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning)
        {
            if (HasScannedMinHorizontalArea())
            {
                SpatialUnderstanding.Instance.RequestFinishScan();
            }
        }
        else if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
        {
            
            if(!m_PlacedCharacter)
            {
                if (position != Vector3.zero)
                {
                    if (m_NavMeshGenerationAtRuntime.IsNavMeshBuilt())
                    {
                        Vector3 hitPosition;
                        SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes surfaceType = GetSurfaceType(m_CameraTransform.position, m_CameraTransform.forward, m_MaxRaycastDistance, out hitPosition);
                        if (surfaceType == SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes.Floor || surfaceType == SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes.FloorLike)
                        {
                            m_PlacedCharacter = Instantiate(m_CharacterToPlace, position, new Quaternion());
                            RotateCharacterToPosition(m_CameraTransform.position);
                            DisableSpatialUnderstandingMeshWire(m_DisableMeshOnScanDone);
                            if (OnPlacedCharacter != null)
                            {
                                OnPlacedCharacter();
                            }
                        }
                    }
                }
            }
        }
    }

    public void OnHoldStarted(HoldEventData eventData)
    {
        m_IsAirTapBeingHeld = true;
        m_IsAirTapBeingHeldTime = 0.0f;

        RaycastHit hit;
        Vector3 position = Vector3.zero;
        if (Physics.Raycast(m_CameraTransform.position, m_CameraTransform.forward, out hit, m_MaxRaycastDistance))
        {
            position = hit.point;
        }

        if (OnStartedHoldingAirTap != null)
        {
            OnStartedHoldingAirTap(position);
        }
    }

    public void OnHoldCompleted(HoldEventData eventData)
    {
        m_IsAirTapBeingHeld = false;
        RaycastHit hit;
        Vector3 position = Vector3.zero;
        if (Physics.Raycast(m_CameraTransform.position, m_CameraTransform.forward, out hit, m_MaxRaycastDistance))
        {
            position = hit.point;
        }
        if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
        {
            if (m_PlacedCharacter)
            {
                if (position != Vector3.zero)
                {
                    Vector3 hitPosition;
                    SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes surfaceType = GetSurfaceType(m_CameraTransform.position, m_CameraTransform.forward, m_MaxRaycastDistance, out hitPosition);
                    Debug.Log("SurfaceType: " + surfaceType);
                    if ((surfaceType == SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes.Floor || surfaceType == SpatialUnderstandingDll.Imports.RaycastResult.SurfaceTypes.FloorLike))
                    {
                        SetCharacterToPosition(position);
                        RotateCharacterToPosition(m_CameraTransform.position);
                    }
                }
            }
        }
        if (OnStoppedHoldingAirTap != null)
        {
            OnStoppedHoldingAirTap(position);
        }
    }

    public void OnHoldCanceled(HoldEventData eventData)
    {
        OnHoldCompleted(eventData);
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

    public bool HasPlacedCharacter()
    {
        if (m_PlacedCharacter)
        {
            return true;
        }
        return false;
    }

    public void RegisterLookAtEvent(LookAtEvent eventToRegister)
    {
        if(eventToRegister)
        {
            m_LookAtEvents.Add(eventToRegister);
            if (OnLookAtEventRegistered != null)
            {
                OnLookAtEventRegistered(eventToRegister);
            }
        }
    }

    public void UnregisterLookAtEvent(LookAtEvent eventToUnregister)
    {
        if (eventToUnregister)
        {
            m_LookAtEvents.Remove(eventToUnregister);
            if (OnLookAtEventUnregistered != null)
            {
                OnLookAtEventUnregistered(eventToUnregister);
            }
        }
    }

    //Plane creation (not needed anymore)

    void CreatePlanes()
    {
        SurfaceMeshesToPlanes surfaceToPlanes = SurfaceMeshesToPlanes.Instance;
        if (surfaceToPlanes != null && surfaceToPlanes.enabled)
        {
            surfaceToPlanes.MakePlanes();
        }
    }

    void OnMakePlanesComplete(object source, EventArgs args)
    {
        m_RoomSetupDelayTimer = m_RoomSetupDelayTime;
    }




    void OnDestroy()
    {
        if(SpatialUnderstanding.Instance)
        {
            SpatialUnderstanding.Instance.ScanStateChanged -= OnScanStateChanged;
        }
        // SurfaceMeshesToPlanes.Instance.MakePlanesComplete -= OnMakePlanesComplete;
    }
}