using UnityEngine;

public abstract class SingletonMono<T> : MonoBehaviour, ISingleton where T : SingletonMono<T>
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

    public static void Init()
    {
        var ins = instance;
        var singletonIns = ins as ISingleton;
        singletonIns?.OnInit();
    }

    public static void DeInit()
    {
        if (m_Instance != null)
        {
            var singletonIns = m_Instance as ISingleton;
            singletonIns?.OnDeInit();
            m_Instance = null;
        }
    }

    public virtual void OnInit()
    {        
    }

    public virtual void OnDeInit()
    {
    }
}
