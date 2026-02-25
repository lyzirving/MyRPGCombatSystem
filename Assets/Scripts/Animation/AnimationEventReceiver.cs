using UnityEngine;
using System.Collections.Generic;

// Avoid reference change
public delegate void AnimationEventHandle(in AnimationEventInfo info);

public class AnimationEventReceiver : SingletonMono<AnimationEventReceiver>
{
    /// <summary>
    /// Key: guid of the listener instance
    /// Value: events and handlers of an instance
    /// </summary>
    private Dictionary<int, Dictionary<AnimationEventType, AnimationEventHandle>> m_InstanceMap;

    public override void OnInit()
    {
        m_InstanceMap = new Dictionary<int, Dictionary<AnimationEventType, AnimationEventHandle>>();
    }

    public override void OnDeInit()
    {
        Debug.Log("AnimationEventReceiver: OnDeInit");
        if (m_InstanceMap != null)
        {
            m_InstanceMap.Clear();
            m_InstanceMap = null;
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
