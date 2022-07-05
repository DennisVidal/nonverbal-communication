using UnityEngine;

public class IntructionsUI : MonoBehaviour
{
    [SerializeField]
    protected string m_ScanningText = "Walk and look around to scan your surroundings";
    [SerializeField]
    protected Color m_ScanningTextColor = Color.red;

    [SerializeField]
    protected string m_ScannedMinAreaText = "Once you scanned your desired playspace,\nAir Tap to finish scanning";
    [SerializeField]
    protected Color m_ScannedMinAreaTextColor = Color.green;

    [SerializeField]
    protected string m_PlaceCharacterText = "Air Tap while looking at the floor to place the character";

    [SerializeField]
    protected Color m_PlaceCharacterTextColor = Color.green;

    [SerializeField]
    protected string m_ControlsThrowText = "Air Tap somewhere to throw an object";
    [SerializeField]
    protected string m_ControlsLookAtPlayer = "Air Tap on the character to make him look at you";
    [SerializeField]
    protected string m_ControlsPointText = "Hold the Air Tap for a bit to point in a direction";
    [SerializeField]
    protected string m_ControlsSetPosition = "Hold the Air Tap for a while and release it to teleport the character";

    [SerializeField]
    protected TextMesh m_TextMesh;

    protected bool m_IsScanningRoom;

    void Start()
    {
        if(!m_TextMesh)
        {
            m_TextMesh = GetComponent<TextMesh>();
        }

        GameManager.Instance.OnStartedScanningRoom += OnStartedScanningRoom;
        GameManager.Instance.OnScannedMinArea += OnScannedMinArea;
        GameManager.Instance.OnFinishedRoomSetup += OnFinishedRoomSetup;
        GameManager.Instance.OnPlacedCharacter += OnPlacedCharacter;
    }

    void Update()
    {
        if(m_IsScanningRoom)
        {
            float percentage = Mathf.Clamp01(GameManager.Instance.GetScannedHorizontalArea() / GameManager.Instance.GetMinHorizontalAreaNeeded());
            m_TextMesh.text = m_ScanningText + "\n" + percentage.ToString("P");
            m_TextMesh.color = m_ScanningTextColor;
        }
    }


    void OnStartedScanningRoom()
    {
        m_IsScanningRoom = true;
    }
    void OnScannedMinArea()
    {
        m_IsScanningRoom = false;
        m_TextMesh.text = m_ScannedMinAreaText;
        m_TextMesh.color = m_ScannedMinAreaTextColor;
    }
    void OnFinishedRoomSetup()
    {
        m_TextMesh.text = m_PlaceCharacterText + "\nControls:" + "\n" + m_ControlsThrowText + "\n" + m_ControlsLookAtPlayer + "\n" + m_ControlsPointText + "\n" + m_ControlsSetPosition;
        m_TextMesh.color = m_PlaceCharacterTextColor;
    }
    void OnPlacedCharacter()
    {
        m_TextMesh.text = "";
        m_TextMesh.color = Color.white;
    }

    void OnDestroy()
    {
        GameManager.Instance.OnStartedScanningRoom -= OnStartedScanningRoom;
        GameManager.Instance.OnScannedMinArea -= OnScannedMinArea;
        GameManager.Instance.OnFinishedRoomSetup -= OnFinishedRoomSetup;
        GameManager.Instance.OnPlacedCharacter -= OnPlacedCharacter;
    }
}
