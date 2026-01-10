using UnityEngine;

public class PlayerStateLand : PlayerStateMove
{
    public override void Enter(StateBase exitState)
    {
        base.Enter(exitState);
        m_Player.model.StartAnimation(m_Player.animConsts.landHash);
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.StopAnimation(m_Player.animConsts.landHash);
        base.Exit(newState);
    }

    public override void Update()
    {
    }
}
