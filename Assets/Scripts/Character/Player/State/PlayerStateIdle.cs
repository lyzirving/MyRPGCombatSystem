using UnityEngine;

public class PlayerStateIdle : PlayerStateBase
{
    public override void Enter()
    {
        m_Player.model.StartAnimation(m_Player.animConsts.idleHash);
    }

    public override void Exit()
    {
        m_Player.model.StopAnimation(m_Player.animConsts.idleHash);
    }
}
