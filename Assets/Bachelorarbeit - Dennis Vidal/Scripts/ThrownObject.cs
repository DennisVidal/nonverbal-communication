using System.Collections.Generic;
using UnityEngine;

public class ThrownObject : MonoBehaviour
{
    [SerializeField]
    protected float m_LifeTime = 60.0f;

    [SerializeField]
    protected float m_MinSize = 0.05f;
    [SerializeField]
    protected float m_MaxSize = 0.1f;


    [SerializeField]
    protected List<Material> m_PossibleMaterials;

    [SerializeField]
    protected AudioSource m_AudioSource;
    [SerializeField]
    protected AudioClip m_HitSomethingSound;
    [SerializeField]
    protected AudioClip m_RollingSound;

    protected Rigidbody m_Rigidbody;

    protected bool m_IsRolling;

    void Start()
    {
        float randomSize = Random.Range(m_MinSize, m_MaxSize);
        transform.localScale = new Vector3(randomSize, randomSize, randomSize);

        MeshRenderer m_MeshRenderer = GetComponent<MeshRenderer>();
        if (m_MeshRenderer)
        {
            m_MeshRenderer.material = GetRandomMaterial();
        }

        if(!m_Rigidbody)
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        m_IsRolling = false;
    }

    void Update()
    {
        m_LifeTime -= Time.deltaTime;
        if (m_LifeTime < 0.0f)
        {
            Destroy(gameObject);
        }
    }

    public Material GetRandomMaterial()
    {
        if(m_PossibleMaterials.Count > 0)
        {
            return m_PossibleMaterials[Random.Range(0, m_PossibleMaterials.Count)];
        }
        return null;
    }

    public bool IsRolling()
    {
        return m_IsRolling;
    }
    public void PlayHitSomethingSound(float volume = -1.0f)
    {
        if (m_HitSomethingSound)
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.clip = m_HitSomethingSound;
                m_AudioSource.loop = false;
                if (volume > 0.0f && volume < 1.0f)
                {
                    m_AudioSource.volume = volume;
                }
                else
                {
                    m_AudioSource.volume = 1.0f;
                }
                m_AudioSource.Play();
            }
        }
    }
    public void PlayRollingSound(float volume = -1.0f)
    {
        if (m_RollingSound)
        {
            if(!m_AudioSource.isPlaying)
            {
                m_AudioSource.clip = m_RollingSound;
                m_AudioSource.loop = true;
                if (volume > 0.0f && volume < 1.0f)
                {
                    m_AudioSource.volume = volume;
                }
                else
                {
                    m_AudioSource.volume = 1.0f;
                }
                m_AudioSource.Play();
            }
        }
    }

    public void StopRollingSound()
    {
        if (m_RollingSound)
        {
            m_AudioSource.loop = false;
            m_AudioSource.volume = 1.0f;
            m_AudioSource.Stop();
        }
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag != "Player")
        {
            float velocity = collision.relativeVelocity.magnitude;
            if(velocity > 0.2f)
            {
                PlayHitSomethingSound(Mathf.Clamp01(velocity));
            }
        }
    }
    protected void OnCollisionStay(Collision collision)
    {
        if (m_Rigidbody)
        {
            if (!m_IsRolling && m_Rigidbody.velocity.magnitude > 0.2f)
            {
                m_IsRolling = true;
                PlayRollingSound(Mathf.Clamp01(m_Rigidbody.velocity.magnitude));
            }
            else if (m_IsRolling && m_Rigidbody.velocity.magnitude < 0.2f)
            {
                m_IsRolling = false;
                StopRollingSound();
            }
        }
    }

    protected void OnCollisionExit(Collision collision)
    {
        if (m_IsRolling)
        {
            m_IsRolling = false;
            StopRollingSound();
        }
    }
}
