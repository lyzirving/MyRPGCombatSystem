using UnityEngine;

public class PlayerStateAirborne : PlayerStateBase
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_Player.model.StartAnimation(m_Player.animConsts.airborneHash);
    }

    public override void Exit(StateBase newState)
    {
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(PlayerStateAirborne)))
        {
            m_Player.model.StopAnimation(m_Player.animConsts.airborneHash);
        }
    }
}
