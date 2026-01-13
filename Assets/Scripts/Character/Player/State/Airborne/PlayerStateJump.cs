using UnityEngine;
using AnimationDefine;

public class PlayerStateJump : PlayerStateAirborne
{
    private bool m_HasJumpStarted = false;

    public override void Enter(StateBase exitState, in ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_HasJumpStarted = false;
        AnimationEventReceiver.instance.RegisterHandler(PlayerAnimationEvent.JumpStart, HandleJumpStart);
        AnimationEventReceiver.instance.RegisterHandler(PlayerAnimationEvent.JumpTop, HandleJumpToTop);
        m_Player.model.StartAnimation(m_Player.animConsts.jumpHash);
        m_Player.model.RegisterRootMotionAction(HandleRootMotion);
    }

    public override void Exit(StateBase newState)
    {
        m_HasJumpStarted = false;
        AnimationEventReceiver.instance.RemoveHandler(PlayerAnimationEvent.JumpStart, HandleJumpStart);
        AnimationEventReceiver.instance.RemoveHandler(PlayerAnimationEvent.JumpTop, HandleJumpToTop);
        m_Player.model.RemoveRootMotionAction(HandleRootMotion);
        m_Player.model.StopAnimation(m_Player.animConsts.jumpHash);
        base.Exit(newState);
    }

    public override void Update()
    {
        if (!m_HasJumpStarted)
            return;

        if (!InputManager.instance.isPlayerMoving)
            return;        
    }

    private void HandleRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
    }

    private void HandleJumpStart()
    {
        m_HasJumpStarted = true;
    }

    private void HandleJumpToTop()
    {
        m_Player.ChangeState(EPlayerState.Falling);
    }
}
