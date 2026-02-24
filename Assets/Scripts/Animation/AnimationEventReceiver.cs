using UnityEngine;
using System.Collections.Generic;

// Avoid reference change
public delegate void AnimationEventHandle(in AnimationEventInfo info);

public class AnimationEventReceiver : SingletonMono<AnimationEventReceiver>
{
    private Dictionary<AnimationEventType, AnimationEventHandle> m_Map;

    /// <summary>
    /// Key: guid of the listener instance
    /// Value: events and handlers of an instance
    /// </summary>
    private Dictionary<int, Dictionary<AnimationEventType, AnimationEventHandle>> m_InstanceMap;

    public override void OnInit()
    {
        m_Map = new Dictionary<AnimationEventType, AnimationEventHandle>();
        m_InstanceMap = new Dictionary<int, Dictionary<AnimationEventType, AnimationEventHandle>>();
    }

    public override void OnDeInit()
    {
        if (m_Map != null)
        {
            Debug.Log("AnimationEventReceiver: OnDeInit");
            m_Map.Clear();
            m_Map = null;            
        }

        if (m_InstanceMap != null)
        {
            m_InstanceMap.Clear();
            m_InstanceMap = null;
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

    public void OnAnimationEventTrigger(in AnimationEventInfo info)
    {
        if (m_Map.ContainsKey(info.type))
        {
            var instance = m_Map[info.type];
            instance?.Invoke(info);
        }
    }

    public void RegisterAction(int guid, AnimationEventType key, AnimationEventHandle action)
    {
        if (m_InstanceMap.TryGetValue(guid, out var instanceMap))
        {
            if (instanceMap.TryGetValue(key, out var handle))
            {
                instanceMap[key] = handle + action;
            }
            else
            {
                instanceMap[key] = action;
            }
        }
        else
        {
            instanceMap = new Dictionary<AnimationEventType, AnimationEventHandle>();
            instanceMap[key] = action;
            m_InstanceMap.Add(guid, instanceMap);
        }
    }

    public void RemoveAction(int guid, AnimationEventType key, AnimationEventHandle action)
    {
        if (m_InstanceMap.TryGetValue(guid, out var map) && map.TryGetValue(key, out var handle))
        {
            map[key] = handle - action;
        }
    }    

    public void OnAnimationEventTrigger(int guid, in AnimationEventInfo info)
    {
        if (m_InstanceMap.TryGetValue(guid, out var map) && map.TryGetValue(info.type, out var handle))
        {
            handle?.Invoke(info);
        }
    }    
}
