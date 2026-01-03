using UnityEngine;

public enum PlayerState : uint
{ 
    Idle = 0,
    Walk,
    Run
}

public class PlayerStateBase : StateBase
{
    protected PlayerController m_Player;

    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        m_Player = owner as PlayerController;
    }
}
