
public class PlayerStateJumpIdle : PlayerStateAirborne
{
    private EFootStep m_FootStep;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_FootStep = args.footStep;
        m_Player.model.StartAnimation(m_Player.animConsts.jumpIdleHash);

        m_Player.resizableCapsule.StoreStepHeightPercent();
        m_Player.resizableCapsule.SetStepHeightPercent(0f);
    }

    public override void Exit(StateBase newState)
    {
        m_Player.resizableCapsule.RestoreStepHeightPercent();
        m_Player.model.StopAnimation(m_Player.animConsts.jumpIdleHash);
        base.Exit(newState);
    }

    public override void Update()
    {
        if (isMovingUp)
            return;

        m_Player.ChangeState(EPlayerState.Falling, new ChangeStateArgs.Builder(m_FootStep).Build());
    }
}
