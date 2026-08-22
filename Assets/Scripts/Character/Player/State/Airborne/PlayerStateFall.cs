using UnityEngine;

public class PlayerStateFall : PlayerStateAirborne
{
    private float m_HoverTime;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        float feetTween = (args.footStep == EFootstep.None) ? 0f : (args.footStep == EFootstep.LeftFootstep) ? 2.1f : -2.1f;
        m_Player.model.SetAnimationFloat(AnimationConsts.feetTween, feetTween);
        m_Player.model.SetAnimationFloat(AnimationConsts.jumpRatio, -4f);
    }

    public override void Update()
    {
        float ratio = m_Player.model.GetAnimationFloat(AnimationConsts.jumpRatio);
        m_Player.model.SetAnimationFloat(AnimationConsts.jumpRatio, ratio - Time.deltaTime);
    }

    public override void FixedUpdate()
    {
        // Poll grounded state every fixed frame instead of relying only on the touch event:
        // the touch callback only fires when the grounded flag *changes*, so a transition that
        // happens while already grounded (e.g. the jump anti-stuck timeout) would never land.
        if (m_Player.sensor.isGrounded)
        {
            m_Player.ChangeState(ECharacterState.Idle);
            return;
        }

        // Anti-freeze: when wedged against an obstacle and hovering (vertical speed near zero
        // for a few frames, e.g. the rigidbody fell asleep on an obstacle's edge), force the
        // character downward so it can never hang in the air forever.
        float v = m_Player.verticalVelocity.y;
        if (IsBlockedAhead(m_Player.transform.forward) && Mathf.Abs(v) < 0.3f)
            m_HoverTime += Time.deltaTime;
        else
            m_HoverTime = 0f;

        if (m_HoverTime > 0.2f)
        {
            m_HoverTime = 0f;
            m_Player.rigidBody.WakeUp();
            m_Player.MoveImmediately(new Vector3(0f, -3f, 0f) - m_Player.verticalVelocity);
        }

        // The fall phase also responds to input with air control (speed factor / rotation
        // damping / acceleration damping, shared with Jump)
        UpdateAirborneMovement();
    }

    /// <summary>
    /// Attempt a double jump while falling (only if enabled for the falling phase).
    /// </summary>
    public override bool TryDoubleJump()
    {
        if (!m_Player.config.jump.allowDoubleJumpWhileFalling)
            return false;

        return TryDoubleJumpInternal(m_Player.config.jump.doubleJumpHeight);
    }
}
