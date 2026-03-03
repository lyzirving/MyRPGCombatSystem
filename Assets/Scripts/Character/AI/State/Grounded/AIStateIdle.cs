
public class AIStateIdle : AIStateLocomotion
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_AIController.attrs.speedModify = 0f;
    }
}
