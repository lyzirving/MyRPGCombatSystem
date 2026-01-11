using UnityEngine;

public class PlayerStateFalling : PlayerStateMove
{
    public override void Enter(StateBase exitState, in ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.animConsts.fallHash);
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.StopAnimation(m_Player.animConsts.fallHash);
        base.Exit(newState);
    }

    public override void Update()
    {        
        if (m_Player.character.isGrounded)
        {
            m_Player.ChangeState(PlayerState.Land);
            return;
        }

        Move();
    }
}
