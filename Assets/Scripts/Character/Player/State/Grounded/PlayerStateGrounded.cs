using UnityEngine;

public class PlayerStateGrounded : PlayerStateBase
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_Player.model.StartAnimation(AnimationConsts.ground);
    }

    public override void Exit(StateBase newState)
    {
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(PlayerStateGrounded)))
        {
            m_Player.model.StopAnimation(AnimationConsts.ground);
        }
    }

    public override void OnExitGround()
    {
        m_Player.ChangeState(ECharacterState.Falling);
    }
}
