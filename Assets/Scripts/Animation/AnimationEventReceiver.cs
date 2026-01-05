using AnimationDefine;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class AnimationEventReceiver : SingletonMono<AnimationEventReceiver>
{
    private Dictionary<PlayerAnimationEvent, UnityEvent> m_Map;

    public override void OnInit()
    {
        m_Map = new Dictionary<PlayerAnimationEvent, UnityEvent>();
    }

    public override void OnDeInit()
    {
        if (m_Map != null)
        {
            Debug.Log("AnimationEventReceiver: OnDeInit");
            foreach (var handle in m_Map.Values)
            {
                handle.RemoveAllListeners();
            }
            m_Map.Clear();
            m_Map = null;            
        }
    }

    public void OnAnimationEventTrigger(PlayerAnimationEvent @event)
    {
        if (m_Map.TryGetValue(@event, out UnityEvent handler))
        {
            handler?.Invoke();
        }
    }

    public void RegisterHandler(PlayerAnimationEvent key, UnityAction handler)
    {
        UnityEvent unityEvent = null;
        if (!m_Map.TryGetValue(key, out unityEvent))
        {
            unityEvent = new UnityEvent();
            m_Map[key] = unityEvent;
        }
        unityEvent.AddListener(handler);
    }

    public void RemoveHandler(PlayerAnimationEvent key, UnityAction handler)
    {
        if (m_Map.TryGetValue(key, out UnityEvent unityEvent))
        {
            unityEvent.RemoveListener(handler);
        }
    }
}
