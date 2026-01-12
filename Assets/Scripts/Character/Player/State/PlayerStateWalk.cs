
public class PlayerStateWalk : PlayerStateMove
{
    public override void Enter(StateBase exitState, in ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.animConsts.walkHash);
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.StopAnimation(m_Player.animConsts.walkHash);
        base.Exit(newState);
    }

    public override void Update()
    {
        if (!CheckPlayerOnGround())
        {
            m_Player.ChangeState(PlayerState.Falling);
            return;
        }

        if (InputManager.instance.shouldPlayerRun)
        {
            m_Player.ChangeState(PlayerState.Run);
            return;
        }

        base.Update();
    }
}
