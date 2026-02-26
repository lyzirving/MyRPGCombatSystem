
public class PlayerStateLand : PlayerStateGrounded
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(AnimationConsts.land);
        AnimationEventReceiver.instance.RegisterAction(GUIDConsts.PlayerAnimation, AnimationEventType.AnimationTransit, HandleLandTransition);

        m_Player.attrs.speedModify = 0f;
        m_Player.attrs.jumpForce = m_Player.config.stationaryJumpForce;
        m_Player.ResetVelocity();
        m_Player.OnFootStep();
    }

    public override void Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveAction(GUIDConsts.PlayerAnimation, AnimationEventType.AnimationTransit, HandleLandTransition);
        m_Player.model.StopAnimation(AnimationConsts.land);
        base.Exit(newState);
    }

    public override void Update()
    {
        if (m_Player.action.isJump)
        {
            m_Player.ChangeState(ECharacterState.Jump);
            return;
        }

        if (m_Player.action.isMoving)
        {
            m_Player.ChangeState(m_Player.action.shouldRun ? ECharacterState.Run : ECharacterState.Walk);
            return;
        }
    }

    private void HandleLandTransition(in AnimationEventInfo info)
    {
        m_Player.ChangeState(ECharacterState.Idle);
    }
}
