using UnityEngine;

public class PlayerStateIdle : PlayerStateGrounded
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {        
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.animConsts.idleHash);

        m_Player.attrs.speedModify = 0f;
        m_Player.attrs.jumpForce = m_Player.config.stationaryJumpForce;
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.StopAnimation(m_Player.animConsts.idleHash);
        base.Exit(newState);
    }

    public override void Update()
    {
        if (InputManager.instance.isPlayerAttackPerformed)
        {
            m_Player.ChangeState(EPlayerState.LightAttack);
            return;
        }

        if (InputManager.instance.isPlayerRollPerformed)
        {
            m_Player.ChangeState(EPlayerState.Roll);
            return;
        }

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
        ResetVelocity();

        Float();        
    }
}
