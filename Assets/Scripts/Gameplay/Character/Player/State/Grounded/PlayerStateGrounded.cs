

public class PlayerStateGrounded : PlayerStateBase
{
    public override void OnExitGround()
    {
        EFootstep footStep = EFootstep.None;
        if (this is PlayerStateMove stateMove)
            footStep = stateMove.CurrentFootstep;
        m_Player.ChangeState(ECharacterState.Falling, new ChangeStateArgs(footStep));
    }
}
