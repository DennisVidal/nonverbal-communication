using UnityEngine;

public class Blink : MonoBehaviour
{
    [SerializeField]
    protected float m_TimeBetweenBlinksMin = 0.5f;
    [SerializeField]
    protected float m_TimeBetweenBlinksMax = 7.0f;
    protected float m_CurrentInBetweenBlinkTimer = 0.0f;

    protected Animator m_Animator;
    void Start()
    {
        m_Animator = gameObject.GetComponent<Animator>();

        m_CurrentInBetweenBlinkTimer = GetNewInBetweenBlinksTime();
    }
    void Update()
    {
        m_CurrentInBetweenBlinkTimer -= Time.deltaTime;
        if(m_CurrentInBetweenBlinkTimer < 0.0f)
        {
            if (!m_Animator.GetBool("IsBlinking"))
            {
                m_Animator.SetBool("IsBlinking", true);
            }
        }
    }
    public float GetNewInBetweenBlinksTime()
    {
        return Random.Range(m_TimeBetweenBlinksMin, m_TimeBetweenBlinksMax);
    }
    public void StartBlinkTimer()//Should be called at the end of the blinking animation
    {
        m_Animator.SetBool("IsBlinking", false);
        m_CurrentInBetweenBlinkTimer = GetNewInBetweenBlinksTime();
    }
}
