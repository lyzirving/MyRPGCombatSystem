using UnityEngine;

public class CharacterStateBase : AdditiveState
{
    protected CharacterControllerBase m_ControllerBase;

    #region State Methods
    public override void Init(IStateMachineOwner owner)
    {
        m_ControllerBase = owner as CharacterControllerBase;
    }
    #endregion

    #region Virtual Methods
    public virtual void OnContactGround(Collider collider) { }

    public virtual void OnExitGround() { }    
    #endregion

    #region Main Methods
    public void Move(in Vector3 force)
    {
        m_ControllerBase.Move(force - m_ControllerBase.horizontalVelocity);
    }

    public void MoveImmediately(in Vector3 force)
    {
        m_ControllerBase.MoveImmediately(force - m_ControllerBase.horizontalVelocity);
    }

    public void Jump(float targetHeight)
    {
        float target = PhysicsUtils.CalcTargetVelocity(0f, Physics.gravity.y, targetHeight);
        Vector3 v = m_ControllerBase.sensor.averageVelocity;
        v.y = target;

        m_ControllerBase.ResetVelocity();
        m_ControllerBase.rigidBody.AddForce(v, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Jump by only resetting the vertical velocity while preserving the current horizontal
    /// momentum. Used by the double jump, which must not discard the air speed already gained.
    /// </summary>
    public void JumpVertical(float targetHeight)
    {
        float target = PhysicsUtils.CalcTargetVelocity(0f, Physics.gravity.y, targetHeight);
        Vector3 velocity = m_ControllerBase.rigidBody.linearVelocity;
        velocity.y = target;
        m_ControllerBase.rigidBody.linearVelocity = velocity;
    }
    #endregion

    /// <summary>
    /// Returns the length (seconds) of the clip currently driving the base layer.
    /// Returns 1f as a safe fallback when the clip can't be resolved, so the speed
    /// calculation never divides by zero or produces a nonsensical value.
    /// </summary>
    protected float GetCurrentClipLength()
    {
        var animator = m_ControllerBase.model.animator;
        if (animator == null)
            return 1f;

        int layer = AnimationConsts.BASE_LAYER;

        // During a transition, read the *next* clip; otherwise the current clip.
        AnimatorClipInfo[] clipInfos = animator.IsInTransition(layer)
            ? animator.GetNextAnimatorClipInfo(layer)
            : animator.GetCurrentAnimatorClipInfo(layer);

        if (clipInfos == null || clipInfos.Length == 0)
            return 1f;

        AnimationClip clip = clipInfos[0].clip;
        if (clip == null || clip.length <= 0f)
            return 1f;

        return clip.length;
    }
}
