using UnityEngine;

public class PlayerStateRoll : PlayerStateMove
{
    private Vector3 m_Direction;
    private bool m_ShouldTransit;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.animConsts.rollHash);
        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.AnimationTransit, HandleRollTransition);

        m_ShouldTransit = false;
        m_Player.attrs.speedModify = m_Player.config.rollSpeedModify;
        m_Direction = GetTargetDirection();
    }

    public override void Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveAction(AnimationEventType.AnimationTransit, HandleRollTransition);
        m_Player.model.StopAnimation(m_Player.animConsts.rollHash);
        base.Exit(newState);
    }

    public override void Update()
    {
        if(!m_ShouldTransit)
            return;

        if (!m_Player.action.isPlayerMoving)
        {
            m_Player.ChangeState(EPlayerState.Idle);
        }
        else
        {
            m_Player.ChangeState(m_Player.action.shouldPlayerRun ? EPlayerState.Run : EPlayerState.Walk);
        }
    }

    public override void FixedUpdate()
    {
        if (m_ShouldTransit)
            return;

        Float();

        MoveAt(m_Direction);
    }

    private void HandleRollTransition(in AnimationEventInfo info)
    {
        m_ShouldTransit = true;
    }
}
