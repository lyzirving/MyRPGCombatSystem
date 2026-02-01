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
        if (m_Player.action.isPlayerAttackPerformed)
        {
            m_Player.ChangeState(EPlayerState.StandardAttack);
            return;
        }

        if (m_Player.action.isPlayerRollPerformed)
        {
            m_Player.ChangeState(EPlayerState.Roll);
            return;
        }

        if (m_Player.action.isPlayerJumpPerformed)
        {
            m_Player.ChangeState(EPlayerState.Jump);
            return;
        }

        if (m_Player.action.isPlayerMoving)
        {
            m_Player.ChangeState(m_Player.action.shouldPlayerRun ? EPlayerState.Run : EPlayerState.Walk);
            return;
        }
    }

    public override void FixedUpdate()
    {
        ResetVelocity();

        Float();        
    }
}
