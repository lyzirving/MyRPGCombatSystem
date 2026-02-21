
public class PlayerStateCombat : PlayerStateGrounded
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(AnimationConsts.combat);
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.StopAnimation(AnimationConsts.combat);
        base.Exit(newState);
    }    
}
