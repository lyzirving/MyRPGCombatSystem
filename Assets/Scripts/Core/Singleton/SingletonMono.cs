using UnityEngine;

public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    private static T m_Instance;

    public static T instance
    {
        get
        {
            if (m_Instance == null)
            {
                var go = new GameObject(typeof(T).Name);
                DontDestroyOnLoad(go);
                m_Instance = go.AddComponent<T>();
            }
            return m_Instance;
        }
    }

    public virtual void Init()
    { 
    }

    public virtual void DeInit()
    {
        if (m_Instance != null)
        {            
            Destroy(m_Instance.gameObject);
            m_Instance = null;
        }
    }
}
