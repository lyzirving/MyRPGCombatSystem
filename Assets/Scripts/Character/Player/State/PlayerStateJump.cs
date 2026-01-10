using UnityEngine;
using AnimationDefine;

public class PlayerStateJump : PlayerStateMove
{
    public override void Enter(StateBase exitState)
    {
        base.Enter(exitState);
        AnimationEventReceiver.instance.RegisterHandler(PlayerAnimationEvent.JumpTop, HandleJumpToTop);
        m_Player.model.StartAnimation(m_Player.animConsts.jumpHash);
        m_Player.model.RegisterRootMotionAction(HandleRootMotion);
    }

    public override void Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveHandler(PlayerAnimationEvent.JumpTop, HandleJumpToTop);
        m_Player.model.RemoveRootMotionAction(HandleRootMotion);
        m_Player.model.StopAnimation(m_Player.animConsts.jumpHash);
        base.Exit(newState);
    }

    public override void Update()
    {
        // must override this method
    }

    private void HandleRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        m_Player.character.Move(deltaPosition);
    }

    private void HandleJumpToTop()
    {
        m_Player.ChangeState(PlayerState.Falling);
    }
}
