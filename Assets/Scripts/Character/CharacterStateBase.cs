using UnityEngine;

public class CharacterStateBase : StateBase
{
    protected CharacterControllerBase m_ControllerBase;

    #region State Methods
    public virtual void OnContactGround(Collider collider) { }

    public virtual void OnExitGround() { }

    public override void Init(IStateMachineOwner owner)
    {
        m_ControllerBase = owner as CharacterControllerBase;
    }
    #endregion

    #region Main Methods
    public void Move(in Vector3 velocity, ForceMode mode = ForceMode.VelocityChange)
    {
        m_ControllerBase.rigidBody.AddForce(velocity - m_ControllerBase.horizontalVelocity, mode);
    }

    public void Jump(float targetHeight)
    {
        float target = PhysicsUtils.CalcTargetVelocity(0f, Physics.gravity.y, targetHeight);
        Vector3 v = m_ControllerBase.sensor.averageVelocity;
        v.y = target;

        m_ControllerBase.ResetVelocity();
        m_ControllerBase.rigidBody.AddForce(v, ForceMode.VelocityChange);
    }
    #endregion
}
