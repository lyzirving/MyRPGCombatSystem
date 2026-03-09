using UnityEngine;

public class VFXInstance : MonoBehaviour
{
    private ParticleSystem m_ParticleSystem;

    private string m_Key;
    private bool m_StartPlay = false;

    public string key
    {
        get => m_Key; 
        set { m_Key = value; }
    }
    public float totalTime => m_ParticleSystem.totalTime;
    public bool isComplete => m_ParticleSystem != null ? !m_ParticleSystem.IsAlive() : true;

    private void Awake()
    {
        m_ParticleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (m_ParticleSystem == null || !m_StartPlay) return;

        if (!m_ParticleSystem.main.loop && isComplete)
        {
            VFXManager.instance.Release(this);
        }
    }

    public void Play(Vector3 position, Quaternion rotation)
    {
        if(m_ParticleSystem == null) return;

        transform.position = position;
        transform.rotation = rotation;

        m_StartPlay = true;

        m_ParticleSystem.Clear();
        m_ParticleSystem.Play();
    }

    public void Reset()
    {
        m_StartPlay = false;
    }
}
