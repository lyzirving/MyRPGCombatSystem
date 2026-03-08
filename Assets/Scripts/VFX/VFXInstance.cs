using UnityEngine;

public class VFXInstance : MonoBehaviour
{
    private ParticleSystem m_ParticleSystem;
    private ParticleSystem[] m_ChildrenParticleSystem;

    private float m_StartTime;
    private bool m_IsPlaying;

    private void Awake()
    {
        m_ParticleSystem = GetComponent<ParticleSystem>();
        m_ChildrenParticleSystem = GetComponentsInChildren<ParticleSystem>();
    }    

    public void Play(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        gameObject.SetActive(true);
        m_IsPlaying = true;
        m_StartTime = Time.time;

        m_ParticleSystem?.Clear();
        m_ParticleSystem?.Play();

        if (m_ChildrenParticleSystem != null && m_ChildrenParticleSystem.Length != 0)
        { 
            for (int i = 0; i < m_ChildrenParticleSystem.Length; i++)
            {
                m_ChildrenParticleSystem[i].Clear();
                m_ChildrenParticleSystem[i].Play();
            }
        }
    }
}
