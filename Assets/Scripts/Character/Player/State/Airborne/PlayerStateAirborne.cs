using UnityEngine;

public class PlayerStateAirborne : PlayerStateBase
{
    public override void Enter(StateBase exitState, in ChangeStateArgs args)
    {
        m_Player.model.StartAnimation(m_Player.animConsts.airborneHash);
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.StopAnimation(m_Player.animConsts.airborneHash);
    }
}
