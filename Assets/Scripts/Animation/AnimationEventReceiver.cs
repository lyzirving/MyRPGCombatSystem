using UnityEngine;
using System.Collections.Generic;

// Avoid reference change
public delegate void AnimationEventHandle(in AnimationEventInfo info);

/// <summary>
/// Receives animation events from AnimationEventTrigger (StateMachineBehaviour) and
/// dispatches them to registered handlers. Uses the Animator component as the routing key.
/// </summary>
public class AnimationEventReceiver : SingletonMono<AnimationEventReceiver>
{
    /// <summary>
    /// Key: Animator component that owns the animation
    /// Value: event-type → handler mapping for that animator
    /// </summary>
    private Dictionary<Animator, Dictionary<AnimationEventType, AnimationEventHandle>> m_InstanceMap;

    public override void OnInit()
    {
        m_InstanceMap = new Dictionary<Animator, Dictionary<AnimationEventType, AnimationEventHandle>>();
    }

    public override void OnDeInit()
    {
        if (m_InstanceMap != null)
        {
            m_InstanceMap.Clear();
            m_InstanceMap = null;
        }
    }

    public void RegisterAction(Animator animator, AnimationEventType key, AnimationEventHandle action)
    {
        if (animator == null) return;

        if (m_InstanceMap.TryGetValue(animator, out var instanceMap))
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
            m_InstanceMap.Add(animator, instanceMap);
        }
    }

    public void RemoveAction(Animator animator, AnimationEventType key, AnimationEventHandle action)
    {
        if (animator == null) return;

        if (m_InstanceMap.TryGetValue(animator, out var map) && map.TryGetValue(key, out var handle))
        {
            map[key] = handle - action;
        }
    }

    public void OnAnimationEventTrigger(Animator animator, in AnimationEventInfo info)
    {
        if (animator == null) return;

        if (m_InstanceMap.TryGetValue(animator, out var map) && map.TryGetValue(info.type, out var handle))
        {
            handle?.Invoke(info);
        }
    }
}
