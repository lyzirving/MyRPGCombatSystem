using UnityEngine;

public class PlayerStateAirborne : PlayerStateBase
{
    /// <summary>
    /// Cached air horizontal velocity: keeps momentum from the moment of leaving the ground,
    /// then converges toward the input target speed every frame (air acceleration damping).
    /// </summary>
    protected Vector3 m_AirHorizontalVelocity;
    protected int m_AirJumpsRemaining = 0;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_Player.model.SetAnimationBool(AnimationConsts.airborne, true);
        // Keep the horizontal momentum from the moment of leaving the ground,
        // so run-jumps and falling feel naturally continuous.
        m_AirHorizontalVelocity = m_Player.horizontalVelocity;

        // Reset the remaining air jumps whenever entering the air from the ground (not when
        // transitioning between two airborne states, e.g. Jump -> Fall).
        if (!(exitState is PlayerStateAirborne))
            ResetAirJumps();
    }

    public override bool Exit(StateBase newState)
    {
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(PlayerStateAirborne)))
        {
            m_Player.model.SetAnimationBool(AnimationConsts.airborne, false);
        }
        return true;
    }

    public override void OnContactGround(Collider collider)
    {
        m_Player.OnFootStep(EFootstep.None);
        m_Player.ChangeState(ECharacterState.Idle);
    }

    public virtual bool TryDoubleJump() => false;    

    /// <summary>
    /// Try to consume one air jump. Returns true when a double jump is still available.
    /// </summary>
    protected bool TryConsumeAirJump()
    {
        if (m_AirJumpsRemaining <= 0) return false;
        m_AirJumpsRemaining--;
        return true;
    }

    /// <summary>
    /// Reset the number of remaining air jumps (called when entering the air from the ground).
    /// </summary>
    protected void ResetAirJumps()
    {
        m_AirJumpsRemaining = m_Player.config.jump.allowDoubleJump ? 1 : 0;
    }

    /// <summary>
    /// Air horizontal movement control (shared by the Jump ascent phase and Fall descent phase):
    /// 1) Independent air speed factor: air target speed = run speed × airControlFactor;
    /// 2) Air rotation damping: uses a dedicated airRotateSpeed (lower than ground rotateSpeed);
    /// 3) Air acceleration damping: linearly converges toward the target speed at a max rate of
    ///    airAcceleration (m/s²) per second (frame-rate independent). Momentum is preserved without
    ///    input, so the character cannot stop instantly or do a 180° turn mid-air.
    /// </summary>
    protected void UpdateAirborneMovement()
    {
        // Without input, keep current momentum as the target (preserve forward drift, no deceleration)
        Vector3 targetVelocity = m_AirHorizontalVelocity;

        if (m_Player.action.isMoving)
        {
            Vector3 targetDir = m_Player.GetTargetDirection();

            // Check both the input direction and the current momentum direction: input is
            // camera-relative and may not point exactly at the obstacle being rammed.
            bool blocked = IsBlockedAhead(targetDir);
            if (!blocked && m_AirHorizontalVelocity.sqrMagnitude > 0.01f)
                blocked = IsBlockedAhead(m_AirHorizontalVelocity.normalized);

            if (blocked)
            {
                // Wedged against an obstacle: stop horizontal velocity immediately (no damping)
                // so the character never keeps ramming it. Movement resumes once the capsule
                // rises past the obstacle top and the check below clears.
                m_Player.ResetHorizontalVelocity();
                m_AirHorizontalVelocity = Vector3.zero;
                return;
            }
            else
            {
                if (!m_Player.model.GetAnimationBool(AnimationConsts.locked))
                    m_Player.RotateToTargetDir(targetDir, m_Player.config.jump.airRotateSpeed);
            
                float airSpeed = (m_Player.action.shouldRun ? m_Player.runSpeedScaler : m_Player.walkSpeedScaler) * m_Player.config.jump.airControlFactor;
                targetVelocity = targetDir * airSpeed;
            }
        }

        // Air acceleration damping: horizontal speed changes toward the target
        // by at most airAcceleration m/s per frame (frame-rate independent)
        m_AirHorizontalVelocity = Vector3.MoveTowards(m_AirHorizontalVelocity, targetVelocity,
            m_Player.config.jump.airAcceleration * Time.deltaTime);

        m_Player.MoveImmediately(m_AirHorizontalVelocity - m_Player.horizontalVelocity);
    }

    /// <summary>
    /// Returns true when movement along the given horizontal direction is blocked by an obstacle
    /// the character has not yet risen above. A forward CapsuleCast provides early detection;
    /// an OverlapCapsule fallback covers the already-touching case (CapsuleCast ignores
    /// overlapping colliders). The ground purely below the capsule is excluded so standing on
    /// flat ground is never treated as blocked.
    /// </summary>
    protected bool IsBlockedAhead(Vector3 direction)
    {
        CapsuleCollider capsule = m_Player.capsule;
        Vector3 center = capsule.bounds.center;
        float halfHeight = capsule.height * 0.5f - capsule.radius;
        Vector3 bottom = center - Vector3.up * halfHeight;
        Vector3 top = center + Vector3.up * halfHeight;
        float detectRadius = capsule.radius + 0.05f; // slightly larger than the collider radius

        // 1) Early scan: stops pushing just before actually touching the wall.
        float scanDistance = detectRadius + m_Player.sensor.averageVelocity.magnitude * Time.deltaTime;
        if (Physics.CapsuleCast(bottom, top, detectRadius, direction, scanDistance, GameConsts.Layer.Walkable))
            return true;

        // 2) Already touching the wall: CapsuleCast ignores overlapping colliders, so fall back
        //    to OverlapCapsule. Any overlap that is not the ground below the feet blocks movement.
        //    Intentionally direction-independent: once wedged against an obstacle the character
        //    must stop pushing entirely, otherwise the physics solver can keep it wedged forever.
        Collider[] overlaps = Physics.OverlapCapsule(bottom, top, detectRadius, GameConsts.Layer.Walkable);
        float capsuleBottomY = capsule.bounds.min.y;
        for (int i = 0; i < overlaps.Length; i++)
        {
            Vector3 closest = overlaps[i].ClosestPoint(center);
            // Exclude only the floor directly under the feet: a contact point below the capsule
            // bottom means the overlap is purely the ground. Anything at capsule level or above
            // (walls, obstacle edges) blocks movement.
            if (closest.y > capsuleBottomY + 0.05f)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Shared double-jump execution: reset the vertical velocity to jump again while keeping
    /// the current horizontal momentum. Returns false when a double jump is not available.
    /// </summary>
    protected bool TryDoubleJumpInternal(float height)
    {
        if (!m_Player.config.jump.allowDoubleJump)
            return false;

        if (!TryConsumeAirJump())
            return false;

        JumpVertical(height);
        m_AirHorizontalVelocity = m_Player.horizontalVelocity;
        return true;
    }
}
