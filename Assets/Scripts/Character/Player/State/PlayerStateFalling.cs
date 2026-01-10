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
        ApplyGravity(0.4f);
        Debug.Log($"PlayerStateFalling is grounded[{m_Player.character.isGrounded}]");
    }
}
