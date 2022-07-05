using UnityEngine;

public class DisableRotation : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Quaternion.identity;
    }
}
