using UnityEngine;

public class PlayerStateRun : PlayerStateMove
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(AnimationConsts.run);

        m_Player.attrs.speedModify = m_Player.config.runSpeedModify;
        m_Player.attrs.jumpForce = m_Player.config.mediumJumpForce;
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.StopAnimation(AnimationConsts.run); 
        base.Exit(newState);
    }

    public override void Update()
    {        
        if (!m_Player.action.shouldRun)
        {
            m_Player.ChangeState(ECharacterState.Walk);
            return;
        }

        base.Update();
    }
}
