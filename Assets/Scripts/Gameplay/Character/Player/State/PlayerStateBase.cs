using UnityEngine;

public class PlayerStateBase : CharacterStateBase
{    
    protected PlayerController m_Player;

    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        m_Player = owner as PlayerController;
    }

    protected void GetCurrentAnimationTimeInfo(out int loop, out float time)
    {
        // Note: Only consider the case when entering the move animation.
        //       If character quits the animation, the Update() will not run.
        if (m_Player.model.animator.IsInTransition(AnimationConsts.BASE_LAYER))
        {
            var state = m_Player.model.animator.GetNextAnimatorStateInfo(AnimationConsts.BASE_LAYER);
            loop = Mathf.FloorToInt(state.normalizedTime);
            time = state.normalizedTime % 1f;
        }
        else
        {
            var state = m_Player.model.animator.GetCurrentAnimatorStateInfo(AnimationConsts.BASE_LAYER);
            loop = Mathf.FloorToInt(state.normalizedTime);
            time = state.normalizedTime % 1f;
        }
    }

    #region Attack Related
    /// <summary>
    /// Before the attack animation starts, instantly snap rotation toward
    /// the soft-lock target (up to 30° correction). If the target is beyond
    /// 30°, attack in the original facing direction.
    /// Does NOT affect movement — only rotation.
    /// </summary>
    protected void SnapToSoftLockTarget()
    {
        // Hard lock already handles facing in FixedUpdate; only snap for soft lock
        if (m_Player.lockTarget != null)
            return;

        Transform softTarget = m_Player.softLockTarget;
        if (softTarget == null)
            return;

        Vector3 toTarget = softTarget.position - m_Player.transform.position;
        toTarget.y = 0;
        if (toTarget.sqrMagnitude < 0.01f)
            return;

        Vector3 targetDir = toTarget.normalized;
        float angle = Vector3.Angle(m_Player.transform.forward, targetDir);

        // Max 30° correction; beyond that, attack in original direction
        const float maxSnapAngle = 30f;
        if (angle > maxSnapAngle)
            return;

        // Smooth rotation (not instant LookRotation) — uses high rotate speed
        m_Player.RotateToTargetDir(targetDir, m_Player.config.move.rotateSpeed * 3f);
    }

    protected void RotateWhenAttack()
    {
        m_Player.ResetHorizontalVelocity();
        Vector3 targetDir;
        if (m_Player.lockTarget != null && m_Player.sensor.distZone.IsZone(EDistanceZone.CloseCombatRange))
        {
            targetDir = m_Player.lockTarget.transform.position - m_Player.transform.position;
            targetDir = targetDir.NormalizeIgnoreY();
        }
        else
        {
            targetDir = m_Player.GetTargetDirection();

            // Soft lock: continuously blend facing toward soft-lock target (30%)
            // so Enter()'s initial snap isn't dragged back to pure input direction.
            Transform softTarget = m_Player.softLockTarget;
            if (softTarget != null)
            {
                Vector3 toTarget = softTarget.position - m_Player.transform.position;
                toTarget.y = 0;
                if (toTarget.sqrMagnitude > 0.01f)
                {
                    targetDir = Vector3.Slerp(targetDir, toTarget.normalized, 0.3f).normalized;
                }
            }
        }
        m_Player.RotateToTargetDir(targetDir, m_Player.config.move.rotateSpeed);
    }
    #endregion
}
