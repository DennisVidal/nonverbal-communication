using UnityEngine;

public class Idle : MonoBehaviour
{
    protected Animator m_Animator;
    void Start()
    {
        m_Animator = gameObject.GetComponent<Animator>();
    }

    public void ChooseNewIdleBlendValue()
    {
        m_Animator.SetFloat("IdleBlendValue", Random.value);
    }
}