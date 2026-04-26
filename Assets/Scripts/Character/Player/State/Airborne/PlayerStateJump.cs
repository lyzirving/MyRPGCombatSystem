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
    private bool m_FirstEnter;
    private bool m_JumpFromMove;
    private float m_StartVelocity;
    private float m_LastVelocity;
    private float m_JumpStartRatio;
    private System.Random m_SysRandom = new System.Random();

    #region State Methods
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);

        m_JumpFromMove = exitState.GetType() == typeof(PlayerStateMove);
        m_IsJumpPerform = false;
        m_FirstEnter = true;
        m_LastVelocity = 0f;

        float feetTween;
        if (m_JumpFromMove)
        {
            feetTween = args.footStep == EFootstep.LeftFootstep ? 1f : -1f;
            feetTween *= m_Player.action.shouldRun ? 3f : 1f;
        }
        else
        {
            feetTween = ((float)m_SysRandom.NextDouble() * 2f - 1f);
        }

        m_JumpStartRatio = (m_JumpFromMove && m_Player.action.shouldRun) ? POWER_JUMP_UP_RATIO : NORMAL_JUMP_UP_RATIO;        
        m_Player.model.SetAnimationFloat(AnimationConsts.jumpRatio, m_JumpStartRatio);
        m_Player.model.SetAnimationFloat(AnimationConsts.feetTween, feetTween);
    }

    public override void Update()
    {
        if(!m_IsJumpPerform)
            return;

        float velocity = m_Player.verticalVelocity.y;
        if (m_FirstEnter)
        {
            m_FirstEnter = false;
            m_StartVelocity = velocity;
        }
        UpdateAnimationRatio(velocity);
        m_LastVelocity = velocity;
    }

    public override void FixedUpdate()
    {
        //TODO: if we have horizontal input before jump, we should check
        //      whether there is an obstacle in that direction in case the jump might fail by physics,
        //      and the OnContactGround won't be called.
        if (!m_IsJumpPerform)
        {
            m_IsJumpPerform = true;
            Jump(m_Player.config.jump.normalHeight);
            return;
        }

        float velocity = m_Player.verticalVelocity.y;
        if (velocity < 0f)
            m_Player.rigidBody.AddForce(Physics.gravity * m_Player.config.jump.fallGravityRatio * Time.deltaTime, ForceMode.VelocityChange);

        UpdateAirborneMovement();
    }

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Jump;
    }
    #endregion

    #region Main Methods
    private void UpdateAnimationRatio(float verticalVelocity)
    {
        float ratio = CalcJumpRatio(verticalVelocity, m_LastVelocity, m_StartVelocity);
        m_Player.model.SetAnimationFloat(AnimationConsts.jumpRatio, ratio, 0.1f, Time.deltaTime);
    }

    private void UpdateAirborneMovement()
    {
        if (!m_Player.action.isMoving)
            return;

        Vector3 targetDir = m_Player.GetTargetDirection();

        m_Player.RotateToTargetDir(targetDir, m_Player.config.move.rotateSpeed);

        float v = m_Player.sensor.averageVelocity.magnitude;
        Move(targetDir * v);
    }
    /// <summary>
    /// when current velocity > 0, character is jumpping up.
    /// when current velocity == 0, character is jumpping at the top
    /// when current velocity < 0, character is falling
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="lastVelocity"></param>
    /// <param name="startVelocity"></param>
    /// <returns>jump ratio for animation</returns>
    private float CalcJumpRatio(float velocity, float lastVelocity, float startVelocity)
    {        
        if (velocity > 0)
            return m_JumpStartRatio - (startVelocity - velocity) / (m_JumpStartRatio - JUMP_TOP_RATIO);
        else
            return JUMP_TOP_RATIO + velocity / (JUMP_TOP_RATIO - FALL_DOWN_RATIO);
    }
    #endregion
}
