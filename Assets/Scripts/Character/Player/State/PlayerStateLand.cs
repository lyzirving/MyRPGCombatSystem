using AnimationDefine;
using UnityEngine;

public class PlayerStateLand : PlayerStateMove
{
    public override void Enter(StateBase exitState)
    {
        base.Enter(exitState);
        m_Player.model.StartAnimation(m_Player.animConsts.landHash);
        AnimationEventReceiver.instance.RegisterHandler(PlayerAnimationEvent.LandFinish, HandleLandFinish);
    }

    public override void Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveHandler(PlayerAnimationEvent.LandFinish, HandleLandFinish);
        m_Player.model.StopAnimation(m_Player.animConsts.landHash);
        base.Exit(newState);
    }

    public override void Update()
    {
        ApplyGravity();
    }

    private void HandleLandFinish()
    {
        if (InputManager.instance.isPlayerMoving)
        {
            m_Player.ChangeState(InputManager.instance.shouldPlayerRun ? PlayerState.Run : PlayerState.Walk);            
        }
        else
        {
            m_Player.ChangeState(PlayerState.Idle);
        }
    }
}
