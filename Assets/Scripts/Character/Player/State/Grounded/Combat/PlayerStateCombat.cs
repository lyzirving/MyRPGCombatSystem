
public class PlayerStateCombat : PlayerStateGrounded
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.animConsts.combatHash);
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.StopAnimation(m_Player.animConsts.combatHash);
        base.Exit(newState);
    }    
}
