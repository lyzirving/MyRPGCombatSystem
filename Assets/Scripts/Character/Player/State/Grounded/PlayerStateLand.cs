
public class PlayerStateLand : PlayerStateGrounded
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.animConsts.landHash);
        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.AnimationTransit, HandleLandTransition);

        m_Player.attrs.speedModify = 0f;
        m_Player.attrs.jumpForce = m_Player.config.stationaryJumpForce;
        ResetVelocity();
        m_Player.OnFootStep();
    }

    public override void Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveAction(AnimationEventType.AnimationTransit, HandleLandTransition);
        m_Player.model.StopAnimation(m_Player.animConsts.landHash);
        base.Exit(newState);
    }

    public override void Update()
    {
        if (m_Player.action.isJump)
        {
            m_Player.ChangeState(EPlayerState.Jump);
            return;
        }

        if (m_Player.action.isMoving)
        {
            m_Player.ChangeState(m_Player.action.shouldRun ? EPlayerState.Run : EPlayerState.Walk);
            return;
        }
    }

    public override void FixedUpdate()
    {
        Float();
    }

    private void HandleLandTransition(in AnimationEventInfo info)
    {
        m_Player.ChangeState(EPlayerState.Idle);
    }
}
