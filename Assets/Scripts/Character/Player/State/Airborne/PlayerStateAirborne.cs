using UnityEngine;

public class PlayerStateAirborne : PlayerStateBase
{
    /// <summary>
    /// Cached air horizontal velocity: keeps momentum from the moment of leaving the ground,
    /// then converges toward the input target speed every frame (air acceleration damping).
    /// </summary>
    protected Vector3 m_AirHorizontalVelocity;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_Player.model.SetAnimationBool(AnimationConsts.airborne, true);
        // Keep the horizontal momentum from the moment of leaving the ground,
        // so run-jumps and falling feel naturally continuous.
        m_AirHorizontalVelocity = m_Player.horizontalVelocity;
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

            if (!m_Player.model.GetAnimationBool(AnimationConsts.locked))
                m_Player.RotateToTargetDir(targetDir, m_Player.config.jump.airRotateSpeed);
            
            float airSpeed = (m_Player.action.shouldRun ? m_Player.runSpeedScaler : m_Player.walkSpeedScaler) * m_Player.config.jump.airControlFactor;
            targetVelocity = targetDir * airSpeed;
        }

        // Air acceleration damping: horizontal speed changes toward the target
        // by at most airAcceleration m/s per frame (frame-rate independent)
        m_AirHorizontalVelocity = Vector3.MoveTowards(m_AirHorizontalVelocity, targetVelocity,
            m_Player.config.jump.airAcceleration * Time.deltaTime);

        m_Player.MoveImmediately(m_AirHorizontalVelocity - m_Player.horizontalVelocity);
    }
}
