using UnityEngine;

public class PlayerStateRun : PlayerStateMove
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.animConsts.runHash);

        m_Player.attrs.speedModify = m_Player.config.runSpeedModify;
        m_Player.attrs.jumpForce = m_Player.config.mediumJumpForce;
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.StopAnimation(m_Player.animConsts.runHash); 
        base.Exit(newState);
    }

    public override void Update()
    {        
        if (!m_Player.action.shouldRun)
        {
            m_Player.ChangeState(EPlayerState.Walk);
            return;
        }

        base.Update();
    }
}
