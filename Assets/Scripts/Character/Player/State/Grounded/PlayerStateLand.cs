using AnimationDefine;
using UnityEngine;

public class PlayerStateLand : PlayerStateGrounded
{
    public override void Enter(StateBase exitState, in ChangeStateArgs args)
    {
        m_Player.model.StartAnimation(m_Player.animConsts.landHash);
        AnimationEventReceiver.instance.RegisterHandler(PlayerAnimationEvent.LandFinish, HandleLandFinish);
    }

    public override void Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveHandler(PlayerAnimationEvent.LandFinish, HandleLandFinish);
        m_Player.model.StopAnimation(m_Player.animConsts.landHash);
    }

    private void HandleLandFinish()
    {
        if (InputManager.instance.isPlayerMoving)
        {
            m_Player.ChangeState(InputManager.instance.shouldPlayerRun ? EPlayerState.Run : EPlayerState.Walk);            
        }
        else
        {
            m_Player.ChangeState(EPlayerState.Idle);
        }
    }
}
