using AnimationDefine;
using UnityEngine;

public class AnimationEventTriggerBehaviour : StateMachineBehaviour
{
    public PlayerAnimationEvent event0 = PlayerAnimationEvent.None;
    [Range(0f, 1f)] public float triggerTime;

    private bool m_IsTriggerEvent = false;
    private int m_LoopCount = 0;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_LoopCount = 0;
        m_IsTriggerEvent = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float currentTime = stateInfo.normalizedTime % 1f;
        int currentLoopCount = Mathf.FloorToInt(stateInfo.normalizedTime);

        // Finish one loop
        if (currentLoopCount > m_LoopCount)
        {
            m_LoopCount = currentLoopCount;
            m_IsTriggerEvent = false;
        }

        if (!m_IsTriggerEvent && event0 != PlayerAnimationEvent.None && currentTime >= triggerTime)
        {
            m_IsTriggerEvent = true;
            AnimationEventReceiver.instance.OnAnimationEventTrigger(event0);
        }
    }
}
