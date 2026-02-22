using System.Collections;
using UnityEngine;

public class AIStateHit : AIStateCombat
{
    private readonly float m_TransitTime = 0.1f;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_AIController.model.StartAnimation(AnimationConsts.hit);
        m_AIController.model.SetLayerWeight(m_AIController.hitLayerIndex, 0.5f);

        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.AnimationTransit, HandleTransit);
    }

    public override void Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveAction(AnimationEventType.AnimationTransit, HandleTransit);
        m_AIController.model.SetLayerWeight(m_AIController.hitLayerIndex, 0f);
        m_AIController.model.StopAnimation(AnimationConsts.hit);
        base.Exit(newState);
    }

    private void HandleChangeState()
    {
        m_AIController.ChangeState(ECharacterState.Idle);
    }

    private void HandleTransit(in AnimationEventInfo info)
    {
        MonoManager.Run(HandleTransit_Enumerator());
    }

    private IEnumerator HandleTransit_Enumerator()
    {
        float startTime = Time.time;
        while (startTime + m_TransitTime > Time.time)
        {
            float ratio = (startTime + m_TransitTime - Time.time) / m_TransitTime;
            m_AIController.model.SetLayerWeight(m_AIController.hitLayerIndex, 0.5f * Mathf.Clamp01(1f - ratio));
            yield return null;
        }
        HandleChangeState();
    }

}
