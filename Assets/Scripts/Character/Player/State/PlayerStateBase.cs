using UnityEngine;

public enum PlayerState : uint
{ 
    Idle = 0,
    Walk,
    Run,
    Jump,
    Falling
}

public class PlayerStateBase : StateBase
{
    protected PlayerController m_Player;

    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        m_Player = owner as PlayerController;
    }

    protected void ApplyGravity(float ratio = 1f)
    {
        m_Player?.character.Move(new Vector3(0f, ratio * m_Player.config.gravity * Time.deltaTime, 0f));
    }
}
