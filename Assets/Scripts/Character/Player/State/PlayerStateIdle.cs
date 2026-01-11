using UnityEngine;

public class PlayerStateIdle : PlayerStateBase
{
    public override void Enter(StateBase exitState, in ChangeStateArgs args)
    {
        m_Player.model.StartAnimation(m_Player.animConsts.idleHash);
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.StopAnimation(m_Player.animConsts.idleHash);
    }

    public override void Update()
    {
        ApplyGravity();

        if (InputManager.instance.isPlayerJumpPerformed)
        {
            m_Player.ChangeState(PlayerState.Jump);
            return;
        }

        if (InputManager.instance.isPlayerMoving)
        {
            m_Player.ChangeState(InputManager.instance.shouldPlayerRun ? PlayerState.Run : PlayerState.Walk);
            return;
        }
    }
}
