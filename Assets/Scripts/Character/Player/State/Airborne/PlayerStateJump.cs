using UnityEngine;

public class PlayerStateJump : PlayerStateAirborne
{
    //[Note] ratio should be mapped width threshold in animator's blend tree
    //       ratio here is a little bit larget than threshold in blend tree to avoid numerical jitter
    private const float POWER_JUMP_UP_RATIO = 3.1f;
    private const float NORMAL_JUMP_UP_RATIO = 2.1f;
    private const float JUMP_TOP_RATIO = 0f;
    private const float FALL_DOWN_RATIO = -2.1f;    

    private bool m_IsJumpPerform;
    private bool m_IsDoubleJumpPerform;
    private bool m_FirstEnter;
    private bool m_JumpFromMove;
    private float m_StartVelocity;
    private float m_JumpStartRatio;
    private float m_AirborneTimer;
    private float m_HoverTime;
    private float m_FeetTween = 1f;
    private System.Random m_SysRandom = new System.Random();

    private EJumpState m_State = EJumpState.Start;

    #region State Methods
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);

        m_State = EJumpState.Start;
        // move or sprint
        m_JumpFromMove = exitState is PlayerStateMove;
        m_IsJumpPerform = false;
        m_IsDoubleJumpPerform = false;
        m_FirstEnter = true;
        m_AirborneTimer = 0f;

        if (m_JumpFromMove)
        {
            m_FeetTween = args.footStep == EFootstep.LeftFootstep ? 1f : -1f;
            m_FeetTween *= m_Player.action.shouldRun ? 3f : 1f;
        }
        else
        {
            m_FeetTween = (float)m_SysRandom.NextDouble() * 2f - 1f;
        }

        m_JumpStartRatio = (m_JumpFromMove && m_Player.action.shouldRun) ? POWER_JUMP_UP_RATIO : NORMAL_JUMP_UP_RATIO;        
        m_Player.model.SetAnimationFloat(AnimationConsts.jumpRatio, m_JumpStartRatio);
        m_Player.model.SetAnimationFloat(AnimationConsts.feetTween, m_FeetTween);
    }

    public override bool Exit(StateBase newState)
    {
        bool isFall = newState is PlayerStateFall;
        if (m_State != EJumpState.Landed && !isFall)
            return false;

        m_Player.model.SetAnimationBool(AnimationConsts.doubleJump, false);
        m_Player.model.SetAnimationBool(AnimationConsts.rightFootstep, false);

        if (isFall)
        {
            base.Exit(newState);
            return true;
        }
        
        return base.Exit(newState);
    }

    public override void Update()
    {
        if(!m_IsJumpPerform)
            return;

        if(m_IsDoubleJumpPerform)
        {           
            m_IsDoubleJumpPerform = false;
            // If double jump is peformed, we need to recapture the initial speed .
            m_FirstEnter = true;
            // the execute order between TryDoubleJump() and Update() is not clear, so we need to wait one frame
            m_Player.model.SetAnimationBool(AnimationConsts.doubleJump, true);
            m_Player.model.SetAnimationBool(AnimationConsts.rightFootstep, m_FeetTween > 0f);
            m_Player.model.SetAnimationFloat(AnimationConsts.doubleJumpRatio, 0f);
            return;
        } 

        float velocity = m_Player.verticalVelocity.y;
        if (m_FirstEnter)
        {
            m_FirstEnter = false;
            m_StartVelocity = velocity;
        }

        UpdateAnimationRatio(velocity);
    }

    public override void FixedUpdate()
    {
        if (!m_IsJumpPerform)
        {
            m_IsJumpPerform = true;
            Jump(m_Player.config.jump.normalHeight);

            // If an obstacle blocks the takeoff direction, drop the horizontal momentum so the
            // character jumps straight up instead of ramming the obstacle and getting wedged.
            Vector3 horizontal = m_Player.horizontalVelocity;
            if (horizontal.sqrMagnitude > 0.01f && IsBlockedAhead(horizontal.normalized))
                m_Player.ResetHorizontalVelocity();

            // Keep in sync with the horizontal momentum applied by Jump() to avoid a one-frame
            // mismatch between the cached air velocity and the rigidbody velocity
            m_AirHorizontalVelocity = m_Player.horizontalVelocity;
            m_State = EJumpState.Airborne;
            m_Player.PlayOneShot(m_Player.config.jump.audio);
            return;
        }

        // Grounded poll (same pattern as PlayerStateFall): the touch callback is edge-triggered,
        // so if the grounded flag was already set when the character touches down while pressed
        // against an obstacle, the jump would otherwise never finish on its own. Vertical
        // velocity is checked so this never fires during the take-off frame.
        if (m_Player.sensor.isGrounded && m_Player.verticalVelocity.y <= 0f)
        {
            m_State = EJumpState.Landed;
            return;
        }

        // Anti-stuck safety net: if the jump somehow never lands (e.g. wedged against an
        // obstacle), force a transition to the Fall state so the state machine never locks up.
        m_AirborneTimer += Time.deltaTime;
        if (m_AirborneTimer > m_Player.config.jump.maxAirborneTime)
        {
            m_Player.ChangeState(ECharacterState.Falling);
            return;
        }

        float velocity = m_Player.verticalVelocity.y;
        if (velocity < 0f)
            m_Player.rigidBody.AddForce(Physics.gravity * m_Player.config.jump.fallGravityRatio * Time.deltaTime, ForceMode.VelocityChange);

        // Anti-freeze: when wedged against an obstacle and hovering (vertical speed near zero
        // for a few frames, e.g. the rigidbody fell asleep on an obstacle's edge), force the
        // character downward so it can never hang in the air forever.
        if (IsBlockedAhead(m_Player.transform.forward) && Mathf.Abs(velocity) < 0.3f)
            m_HoverTime += Time.deltaTime;
        else
            m_HoverTime = 0f;

        if (m_HoverTime > 0.2f)
        {
            m_HoverTime = 0f;
            m_Player.rigidBody.WakeUp();
            m_Player.MoveImmediately(new Vector3(0f, -3f, 0f) - m_Player.verticalVelocity);
        }

        UpdateAirborneMovement();
    }

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Jump;
    }

    public override void OnContactGround(Collider collider)
    {
        m_State = EJumpState.Landed;
        m_Player.OnFootStep(EFootstep.None);
    }

    public override bool IsExpired()
    {
        return m_State == EJumpState.Landed;
    }

    /// <summary>
    /// Attempt a double jump while in the jump's airborne phase.
    /// </summary>
    public override bool TryDoubleJump()
    {
        // Only the airborne phase (not take-off or landing) can double jump.
        if (m_State != EJumpState.Airborne)
            return false;

        if (!TryDoubleJumpInternal(m_Player.config.jump.doubleJumpHeight))
            return false;

        m_State = EJumpState.DoubleJump;
        m_IsDoubleJumpPerform = true;
        return true;
    }
    #endregion

    #region Main Methods
    private void UpdateAnimationRatio(float verticalVelocity)
    {        
        if (m_State == EJumpState.DoubleJump)
        {
            float ratio = CalcDoubleJumpRatio(verticalVelocity, m_StartVelocity);
            m_Player.model.SetAnimationFloat(AnimationConsts.doubleJumpRatio, ratio, 0.1f, Time.deltaTime);
        }
        else
        {
            float ratio = CalcJumpRatio(verticalVelocity, m_StartVelocity);
            m_Player.model.SetAnimationFloat(AnimationConsts.jumpRatio, ratio, 0.1f, Time.deltaTime);
        }
    }

    /// <summary>
    /// when current velocity > 0, character is jumpping up.
    /// when current velocity == 0, character is jumpping at the top
    /// when current velocity < 0, character is falling
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="startVelocity"></param>
    /// <returns>jump ratio for animation</returns>
    private float CalcJumpRatio(float velocity, float startVelocity)
    {        
        if (velocity > 0)
            return m_JumpStartRatio - (startVelocity - velocity) / (m_JumpStartRatio - JUMP_TOP_RATIO);
        else
            return JUMP_TOP_RATIO + velocity / (JUMP_TOP_RATIO - FALL_DOWN_RATIO);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="startVelocity"></param>
    /// <returns></returns>
    private float CalcDoubleJumpRatio(float velocity, float startVelocity)
    {        
        if (velocity > 0)
            return Mathf.Clamp01(1f- velocity / startVelocity);
        else
            return 1f;
    }
    #endregion
}
