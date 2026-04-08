using UnityEngine;

public class PlayerStateAirborne : PlayerStateBase
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_Player.model.StartAnimation(AnimationConsts.airborne);
    }

    public override bool Exit(StateBase newState)
    {
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(PlayerStateAirborne)))
        {
            m_Player.model.StopAnimation(AnimationConsts.airborne);
        }
        return true;
    }

    public override void OnContactGround(Collider collider)
    {
        m_Player.OnFootStep(EFootstep.None);
        m_Player.ChangeState(ECharacterState.Idle);
    }
}
