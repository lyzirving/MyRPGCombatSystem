using UnityEngine;

public class PlayerStateBase : CharacterStateBase
{    
    protected PlayerController m_Player;

    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        m_Player = owner as PlayerController;
    }
}
