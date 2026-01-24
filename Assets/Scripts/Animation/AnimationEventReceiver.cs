using UnityEngine;
using System.Collections.Generic;

public delegate void AnimationEventHandle(AnimationEventInfo info);

public class AnimationEventReceiver : SingletonMono<AnimationEventReceiver>
{
    private Dictionary<AnimationEventType, AnimationEventHandle> m_Map;

    public override void OnInit()
    {
        m_Map = new Dictionary<AnimationEventType, AnimationEventHandle>();
    }

    public override void OnDeInit()
    {
        if (m_Map != null)
        {
            Debug.Log("AnimationEventReceiver: OnDeInit");
            m_Map.Clear();
            m_Map = null;            
        }
    }  

    public void RegisterAction(AnimationEventType key, AnimationEventHandle action)
    {
        if (!m_Map.ContainsKey(key))
        {
            m_Map.Add(key, null);
            m_Map[key] = action;
        }
        else
        {
            var instance = m_Map[key];
            m_Map[key] = instance + action;
        }
    }

    public void RemoveAction(AnimationEventType key, AnimationEventHandle action)
    {
        if (m_Map.ContainsKey(key))
        {
            var instance = m_Map[key];
            m_Map[key] = instance - action;
        }
    }

    public void OnAnimationEventTrigger(AnimationEventInfo info)
    {
        if (m_Map.ContainsKey(info.type))
        {            
            var instance = m_Map[info.type];
            instance?.Invoke(info);
        }
    }
}
