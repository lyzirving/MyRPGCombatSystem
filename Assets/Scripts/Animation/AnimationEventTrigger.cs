using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class AnimationEventInfo : IComparable<AnimationEventInfo>
{
    public AnimationEventType type = AnimationEventType.None;
    public float launchTime = 0f; // normalized time when the event is triggered
    public bool triggerOnce = false;

    public int loopCnt = 0;    
    public bool hasTriggered = false;
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

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        for (int i = 0; i < events.Count; i++)
        {
            var e = events[i];
            e.loopCnt = 0;
            e.hasTriggered = false;
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float time = stateInfo.normalizedTime % 1f;
        int loop = Mathf.FloorToInt(stateInfo.normalizedTime);
        for (int i = 0; i < events.Count; i++)
        {
            var e = events[i];
            if (loop > e.loopCnt)
            {
                e.loopCnt = loop;
                if (!(e.hasTriggered && e.triggerOnce))
                    e.hasTriggered = false;
            }

            if (e.type != AnimationEventType.None && !e.hasTriggered && time >= e.launchTime)
            {
                e.triggerTime = time;
                AnimationEventReceiver.instance.OnAnimationEventTrigger(e);
                e.hasTriggered = true;
            }
        }
    }
}
