using UnityEngine;

public class AIStateGround : AIStateBase
{
    public override void OnExitGround()
    {
        m_AIController.ChangeState(ECharacterState.Falling);
    }
}
