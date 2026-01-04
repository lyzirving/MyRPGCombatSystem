using UnityEngine;
using AnimationDefine;

public class DoubleAnimationEventTriggerBehaviour : StateMachineBehaviour
{
    public PlayerAnimationEvent event0 = PlayerAnimationEvent.None;
    [Range(0f, 1f)] public float triggerTime0;

    public PlayerAnimationEvent event1 = PlayerAnimationEvent.None;
    [Range(0f, 1f)] public float triggerTime1;

    private bool m_IsTriggerEvent0 = false;
    private bool m_IsTriggerEvent1 = false;

    private int m_LoopCount = 0;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_LoopCount = 0;
        ResetTrigger();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float currentTime = stateInfo.normalizedTime % 1f;
        int currentLoopCount = Mathf.FloorToInt(stateInfo.normalizedTime);

        // Finish one loop
        if (currentLoopCount > m_LoopCount)
        {
            m_LoopCount = currentLoopCount;
            ResetTrigger();
        }        

        if (!m_IsTriggerEvent0 && event0 != PlayerAnimationEvent.None && currentTime >= triggerTime0)
        {
            m_IsTriggerEvent0 = true;
            AnimationEventReceiver.instance.OnAnimationEventTrigger(event0);
        }

        if (!m_IsTriggerEvent1 && event1 != PlayerAnimationEvent.None && currentTime >= triggerTime1)
        {
            m_IsTriggerEvent1 = true;
            AnimationEventReceiver.instance.OnAnimationEventTrigger(event1);
        }
    }

    private void ResetTrigger()
    {
        m_IsTriggerEvent0 = m_IsTriggerEvent1 = false;
    }
}
