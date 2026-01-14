using UnityEngine;

public class PlayerStateBase : StateBase
{
    public Vector3 playerHorizonVelocity 
    {
        get 
        {
            var velocity = m_Player.rigidBody.linearVelocity;
            velocity.y = 0f;
            return velocity;
        }
    }

    public Vector3 playerVerticalVelocity
    {
        get
        {
            return new Vector3(0f, m_Player.rigidBody.linearVelocity.y, 0f);
        }
    }

    public bool isMoveHorizontally
    {
        get
        {
            var vel = playerHorizonVelocity;
            return new Vector2(vel.x, vel.z).magnitude > Mathf.Epsilon;
        }
    }

    protected PlayerController m_Player;

    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        m_Player = owner as PlayerController;
    }

    protected void ResetVelocity()
    {
        m_Player.rigidBody.linearVelocity = Vector3.zero;
    }
}
