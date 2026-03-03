using UnityEngine;

public class PlayerStateGrounded : PlayerStateBase
{
    public override void OnExitGround()
    {
        m_Player.ChangeState(ECharacterState.Falling);
    }
}
