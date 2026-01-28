
public class PlayerStateLand : PlayerStateGrounded
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.animConsts.landHash);
        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.LandFinish, HandleLandFinish);

        m_Player.attrs.speedModify = 0f;
        m_Player.attrs.jumpForce = m_Player.config.stationaryJumpForce;
        ResetVelocity();
        m_Player.OnFootStep();
    }

    public override void Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveAction(AnimationEventType.LandFinish, HandleLandFinish);
        m_Player.model.StopAnimation(m_Player.animConsts.landHash);
        base.Exit(newState);
    }

    public override void Update()
    {
        if (InputManager.instance.isPlayerJumpPerformed)
        {
            m_Player.ChangeState(EPlayerState.Jump);
            return;
        }

        if (InputManager.instance.isPlayerMoving)
        {
            m_Player.ChangeState(InputManager.instance.shouldPlayerRun ? EPlayerState.Run : EPlayerState.Walk);
            return;
        }
    }

    public override void FixedUpdate()
    {
        Float();
    }

    private void HandleLandFinish(in AnimationEventInfo info)
    {
        m_Player.ChangeState(EPlayerState.Idle);
    }
}
