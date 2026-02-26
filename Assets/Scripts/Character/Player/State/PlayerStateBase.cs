using UnityEngine;

public class PlayerStateBase : CharacterStateBase
{    
    protected PlayerController m_Player;

    #region State Methods
    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        m_Player = owner as PlayerController;
    }

    public override void HandleColliderCheck()
    {
        bool isGrounded = m_Player.sensor.SphereCheckGround(GameConsts.WalkableLayer, out RaycastHit hit);
        if (isGrounded != m_Player.sensor.isGrounded)
        {
            m_Player.sensor.isGrounded = isGrounded;
            if (m_Player.sensor.isGrounded)
            {
                OnContactGround(hit.collider);
            }
            else
            {
                OnExitGround();
            }
        }
    }
    #endregion
}
