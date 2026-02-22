
public class AIStateIdle : AIStateGround
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_AIController.model.StartAnimation(AnimationConsts.idle);

        m_AIController.attrs.speedModify = 0f;
    }

    public override void Exit(StateBase newState)
    {
        m_AIController.model.StopAnimation(AnimationConsts.idle);
        base.Exit(newState);
    }
}
