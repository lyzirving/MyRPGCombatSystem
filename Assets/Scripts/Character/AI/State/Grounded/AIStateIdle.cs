
public class AIStateIdle : AIStateGround
{
    public override void FixedUpdate()
    {
        m_AIController.ResetVelocity();
        m_AIController.Floating();
    }
}
