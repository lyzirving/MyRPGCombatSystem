
public class PlayerStateRun : PlayerStateMove
{
    public override void Enter(StateBase exitState)
    {
        base.Enter(exitState);
        m_Player.model.StartAnimation(m_Player.animConsts.runHash);
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.StopAnimation(m_Player.animConsts.runHash); ;
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(PlayerStateMove)))
        {
            base.Exit(newState);
        }
    }
}
