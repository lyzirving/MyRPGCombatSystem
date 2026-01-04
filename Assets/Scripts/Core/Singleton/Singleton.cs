public abstract class Singleton<T> : ISingleton where T : class, new()
{
    private static T m_Instance;

    public static T instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new T();
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
