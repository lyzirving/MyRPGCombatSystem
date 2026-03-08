
public class PlayerStateGrounded : PlayerStateBase
{
    public override void OnExitGround()
    {
        EFootstep footStep = EFootstep.None;
        if (this.GetType() == typeof(PlayerStateMove))
            footStep = (this as PlayerStateMove).currentFoopStep;
        m_Player.ChangeState(ECharacterState.Falling, new ChangeStateArgs(footStep));
    }
}
