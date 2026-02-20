using UnityEngine;

public class PlayerStateGrounded : PlayerStateBase
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_Player.model.StartAnimation(m_Player.animConsts.groundHash);
    }

    public override void Exit(StateBase newState)
    {
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(PlayerStateGrounded)))
        {
            m_Player.model.StopAnimation(m_Player.animConsts.groundHash);
        }
    }

    protected override void OnExitGround(Collider collider)
    {
        m_Player.ChangeState(ECharacterState.Falling);
    }
}
