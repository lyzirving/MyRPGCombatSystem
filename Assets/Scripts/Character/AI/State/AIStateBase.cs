
using UnityEngine;

public class AIStateBase : CharacterStateBase
{
    protected AIController m_AIController;

    #region State Methods
    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        m_AIController = owner as AIController;
    }

    public override void HandleColliderCheck()
    {
        bool isGrounded = m_AIController.sensor.SphereCheckGround(GameConsts.WalkableLayer, out RaycastHit hit);
        if (isGrounded != m_AIController.sensor.isGrounded)
        {
            m_AIController.sensor.isGrounded = isGrounded;
            if (m_AIController.sensor.isGrounded)
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
