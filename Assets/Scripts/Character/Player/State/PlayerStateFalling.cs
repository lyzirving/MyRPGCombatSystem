using UnityEngine;

public class PlayerStateFalling : PlayerStateMove
{
    public override void Enter(StateBase exitState)
    {
        base.Enter(exitState);
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

        Vector3 speed = m_Player.attrs.moveHorizonSpeed;
        speed.y = m_Player.config.floatingRatio * m_Player.config.gravity;

        m_Player.character.Move(speed * Time.deltaTime);
    }
}
