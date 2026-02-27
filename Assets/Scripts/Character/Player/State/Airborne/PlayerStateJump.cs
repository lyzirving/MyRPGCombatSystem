using UnityEngine;

public class PlayerStateJump : PlayerStateAirborne
{
    private const float k_JumpUpRatio = 2.1f;
    private const float k_JumpTopRatio = 0f;
    private const float k_FallDownRatio = -2.1f;

    private bool m_IsJumpPerform;
    private bool m_FirstEnter;
    private float m_JumpStartVelocity;

    #region State Methods
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        
        m_IsJumpPerform = false;
        m_FirstEnter = true;
        m_Player.model.SetAnimationFloat(AnimationConsts.jumpRatio, k_JumpUpRatio);
    }

    public override void Exit(StateBase newState)
    {
        base.Exit(newState);
    }

    public override void Update()
    {
        if(!m_IsJumpPerform)
            return;

        float currentVelocity = m_Player.verticalVelocity.y;
        if (m_FirstEnter)
        {
            m_FirstEnter = false;
            m_JumpStartVelocity = currentVelocity;
        }
        float ratio = CalcJumpRatio(currentVelocity, m_JumpStartVelocity);
        m_Player.model.SetAnimationFloat(AnimationConsts.jumpRatio, ratio, 0.1f, Time.deltaTime);
    }

    public override void FixedUpdate()
    {
        if (!m_IsJumpPerform)
        {
            m_IsJumpPerform = true;
            Jump();
            return;
        }

        float currentVelocity = m_Player.verticalVelocity.y;
        if (currentVelocity < 0f)
            m_Player.rigidBody.AddForce(Physics.gravity * GameSettings.characterConfig.fallGravityRatio * Time.deltaTime, ForceMode.VelocityChange);
    }

    public override void OnContactGround(Collider collider)
    {
        m_Player.ChangeState(ECharacterState.Land);
    }
    #endregion

    #region Main Methods
    private void Jump()
    {
        Vector3 jumpDirection = m_Player.transform.up;
        float force = PhysicsUtils.CalcVelocity(0f, Physics.gravity.y, GameSettings.characterConfig.idleJumpHeight);

        m_Player.ResetVelocity();
        m_Player.rigidBody.AddForce(force * jumpDirection, ForceMode.VelocityChange);
    }

    /// <summary>
    /// when current velocity > 0, character is jumpping up.
    /// when current velocity == 0, character is jumpping at the top
    /// when current velocity < 0, character is falling
    /// </summary>
    /// <param name="currentV"></param>
    /// <param name="jumpStartV"></param>
    /// <returns>jump ratio for animation</returns>
    private float CalcJumpRatio(float currentV, float jumpStartV)
    {        
        if (currentV > 0)
            return k_JumpUpRatio - (jumpStartV - currentV) / (k_JumpUpRatio - k_JumpTopRatio);
        else
            return k_JumpTopRatio + (currentV) / (k_JumpTopRatio - k_FallDownRatio);
    }
    #endregion
}
