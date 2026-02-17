using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class AnimationEventInfo : IComparable<AnimationEventInfo>
{
    public AnimationEventType type = AnimationEventType.None;
    public float launchTime = 0f; // normalized time when the event is triggered
    public float triggerTime = 0f;

    public int CompareTo(AnimationEventInfo other)
    {
        if (this.launchTime < other.launchTime)
            return -1;

        if (Mathf.Approximately(this.launchTime, other.launchTime))
            return 0;

        return 1;
    }
}

public class AnimationEventTrigger : StateMachineBehaviour
{  
    public List<AnimationEventInfo> events = new List<AnimationEventInfo>();

    private int m_Loop = -1;
    private int m_LastLoop = -1;
    // Current index of event that should be handled
    // Events should be ordered by launch time
    private int m_Index = 0;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        OnStartState();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float time = stateInfo.normalizedTime % 1f;
        m_Loop = Mathf.FloorToInt(stateInfo.normalizedTime); 
        
        if (m_Loop != m_LastLoop)
        {
            m_LastLoop = m_Loop;
            m_Index = 0;
        }

        if (m_Index >= events.Count)
            return;

        var curEvent = events[m_Index];

        if (curEvent.type == AnimationEventType.None)
        {
            ++m_Index;
            return;
        }

        if (time < curEvent.launchTime)
            return;      

        curEvent.triggerTime = time;
        AnimationEventReceiver.instance.OnAnimationEventTrigger(curEvent);

        ++m_Index;
    }

    private void OnStartState()
    {
        m_Loop = -1;
        m_LastLoop = -1;
        m_Index = 0;
    }
}
