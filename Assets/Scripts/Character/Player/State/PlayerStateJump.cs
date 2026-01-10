using UnityEngine;
using AnimationDefine;

public class PlayerStateJump : PlayerStateMove
{
    private bool m_HasJumpStarted = false;

    public override void Enter(StateBase exitState)
    {
        m_HasJumpStarted = false;
        base.Enter(exitState);
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
        {
            m_Player.attrs.moveHorizonSpeed = Vector3.zero;
            return;
        }

        Move(false);
    }

    private void HandleRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        m_Player.character.Move(deltaPosition * m_Player.config.jumpRatio);
    }

    private void HandleJumpStart()
    {
        m_HasJumpStarted = true;
    }

    private void HandleJumpToTop()
    {
        m_Player.ChangeState(PlayerState.Falling);
    }
}
