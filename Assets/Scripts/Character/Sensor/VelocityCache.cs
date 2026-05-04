using UnityEngine;

public class VelocityCache
{
    private const int SPEED_CACHE_NUM = 3;

    private Vector3[] m_VelocityCache;
    private Vector3 m_VelocitySum = Vector3.zero;
    private int m_CacheIndex = 0;

    private Rigidbody m_Rigidbody;

    public Vector3 averageVelocity => m_VelocitySum / SPEED_CACHE_NUM;

    public VelocityCache(Rigidbody rigidbody)
    {
        m_VelocitySum = Vector3.zero;
        m_VelocityCache = new Vector3[SPEED_CACHE_NUM];
        for (int i = 0; i < SPEED_CACHE_NUM; ++i)
            m_VelocityCache[i] = Vector3.zero;

        m_CacheIndex = 0;
        m_Rigidbody = rigidbody;
    }

    public void UpdateVelocity()
    {
        m_VelocitySum -= m_VelocityCache[m_CacheIndex];
        m_VelocityCache[m_CacheIndex] = m_Rigidbody.linearVelocity;
        m_VelocitySum += m_VelocityCache[m_CacheIndex];
        m_CacheIndex = (m_CacheIndex + 1) % SPEED_CACHE_NUM;
    }
}
