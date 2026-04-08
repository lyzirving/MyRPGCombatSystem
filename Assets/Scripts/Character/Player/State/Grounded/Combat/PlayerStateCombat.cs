
public class PlayerStateCombat : PlayerStateGrounded
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_Player.model.StartAnimation(AnimationConsts.combat);
    }

    public override bool Exit(StateBase newState)
    {
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(PlayerStateCombat)))
        {
            m_Player.model.StopAnimation(AnimationConsts.combat);
        }
        return true;
    }    
}
