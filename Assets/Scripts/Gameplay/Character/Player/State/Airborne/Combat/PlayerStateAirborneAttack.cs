using UnityEngine;

public class PlayerStateAirborneAttack : PlayerStateAirborneCombat
{    
    public virtual EFootstep CurrentFootstep
    {
        get 
        {
            return Mathf.Approximately(m_Footstep, 0f) ? EFootstep.None : (m_Footstep > 0f ? EFootstep.LeftFootstep : EFootstep.RightFootstep);
        }
    }

    private AirborneAttackAbility CurrentAttack => m_Player.abilitySystemComp.GetActive<AirborneAttackAbility>();

    private SkillData CurrentSkill => m_Player.attackComponent.skill;

    public float CurrentNormalizedTime => m_NormalizedTime;

    private float m_NormalizedTime = 0;
    private float m_Footstep = 0f;
    private bool m_IsPlunge = false;    

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_NormalizedTime = 0f;
        //Note: plunge is not implemented! it's always false.
        m_IsPlunge = CurrentSkill.attackBehavior == EAttackBehavior.Plunge;        

        // Soft-lock snap: instantly face toward soft-lock target before attack (max 30°)
        SnapToSoftLockTarget();

        m_Player.model.SetAnimationBool(AnimationConsts.plunge, m_IsPlunge);        
        if(!m_IsPlunge)
            m_Player.model.StartAnimation(m_Player.attackComponent.skill.animatorState, m_Player.attackComponent.skill.crossFadeInTime);
            
        var ability = CurrentAttack;
        if (ability != null)
        {
            AnimationEventReceiver.instance.RegisterAction(m_Player.model.animator, AnimationEventType.AttackVfxBegin, ability.HandleAttackVfxBegin);
            AnimationEventReceiver.instance.RegisterAction(m_Player.model.animator, AnimationEventType.AttackVfxEnd, ability.HandleAttackVfxEnd);
            AnimationEventReceiver.instance.RegisterAction(m_Player.model.animator, AnimationEventType.AttackStart, ability.HandleAttackBegin);
            AnimationEventReceiver.instance.RegisterAction(m_Player.model.animator, AnimationEventType.AttackEnd, ability.HandleAttackEnd);
            AnimationEventReceiver.instance.RegisterAction(m_Player.model.animator, AnimationEventType.AttackComboWindowOpened, ability.HandleAttackComboWindowOpened);            
        }
        else
        {
            Debug.LogError($"PlayerStateAirborneAttack::Enter() - no active AirborneAttackAbility found on player[{m_Player.name}]");
        }
    }

    public override void ReEnter(ChangeStateArgs args)
    {        
        m_IsPlunge = CurrentSkill.attackBehavior == EAttackBehavior.Plunge;
        m_NormalizedTime = 0f;
        
        m_Player.ResetAirAttack();

        m_Player.model.SetAnimationBool(AnimationConsts.plunge, m_IsPlunge);
        if(!m_IsPlunge)
            m_Player.model.StartAnimation(m_Player.attackComponent.skill.animatorState, m_Player.attackComponent.skill.crossFadeInTime);        
    }

    public override bool Exit(StateBase newState)
    {
        var ability = CurrentAttack;
        if(m_IsPlunge)
        {
            m_Player.OnAttackEnd();
            m_Player.OnAttackVfxEnd();
        }
        
        m_IsPlunge = false;
        m_Player.model.SetAnimationBool(AnimationConsts.plunge, m_IsPlunge);
        m_Player.attackComponent.EndCombo();
        
        if (ability != null)
        {
            AnimationEventReceiver.instance.RemoveAction(m_Player.model.animator, AnimationEventType.AttackVfxBegin, ability.HandleAttackVfxBegin);
            AnimationEventReceiver.instance.RemoveAction(m_Player.model.animator, AnimationEventType.AttackVfxEnd, ability.HandleAttackVfxEnd);
            AnimationEventReceiver.instance.RemoveAction(m_Player.model.animator, AnimationEventType.AttackStart, ability.HandleAttackBegin);
            AnimationEventReceiver.instance.RemoveAction(m_Player.model.animator, AnimationEventType.AttackEnd, ability.HandleAttackEnd);
            AnimationEventReceiver.instance.RemoveAction(m_Player.model.animator, AnimationEventType.AttackComboWindowOpened, ability.HandleAttackComboWindowOpened);
        }
        else
        {
            Debug.LogError($"PlayerStateAirborneAttack::Exit() - no active AttackAbility found on player[{m_Player.name}]");
        }

        base.Exit(newState);
        return true;
    }

    public override void Update()
    {
        UpdateAnimation();
    }

    public override void FixedUpdate()
    {
        // Poll grounded state every fixed frame instead of relying only on the touch event:
        // the touch callback only fires when the grounded flag *changes*, so a transition that
        // happens while already grounded (e.g. the jump anti-stuck timeout) would never land.
        if (m_Player.sensor.isGrounded)
        {
            HandleLanding();
            return;
        }

        ApplyVerticalPhysics();    
        ApplyHorizontalControl();  
    }

    public override bool IsExpired()
    {
        return m_NormalizedTime >= m_Player.attackComponent.skill.transitionNormalizedTime;
    }

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Attack;
    }

    public override void OnContactGround(Collider collider)
    {
        m_Player.OnFootStep(EFootstep.None);
        CurrentAttack.EndAbility();
    }

    private void UpdateAnimation()
    {
        if (!m_IsPlunge)
        {
            if (!IsExpired())
            {
                m_Player.model.animator.GetTargetAnimationTime(m_Player.attackComponent.skill.animatorState, AnimationConsts.BASE_LAYER, out m_NormalizedTime);
                SampleFootstep();
            }
        }
    }

    /// <summary>
    /// Sample the "footstep" curve from the current air-attack clip.
    /// The curve is bound to PlayerModel.footstep and driven automatically by the Animator,
    /// so reading the field at this moment is the sample value.
    /// </summary>
    private void SampleFootstep()
    {
        // Default: reset before sampling so stale values never leak into m_Footstep.
        m_Footstep = 0f;

        var animator = m_Player.model.animator;
        if (animator == null)
            return;

        int layer = AnimationConsts.BASE_LAYER;

        // Match GetTargetAnimationTime's behaviour: during a transition, sample the *next* clip,
        // otherwise sample the current clip.
        AnimatorClipInfo[] clipInfos = animator.IsInTransition(layer)
            ? animator.GetNextAnimatorClipInfo(layer)
            : animator.GetCurrentAnimatorClipInfo(layer);

        if (clipInfos == null || clipInfos.Length == 0)
            return;

        AnimationClip clip = clipInfos[0].clip;
        if (clip == null)
            return;

        // The curve drives PlayerModel.footstep automatically; just read the current value.
        m_Footstep = (m_Player.model as PlayerModel)?.footstep ?? 0;
        //Debug.Log($"foot step {m_Footstep}");
    }

    private void ApplyVerticalPhysics()
    {
        if (m_IsPlunge)
        {
            // additional downward force to accelerated descent.
            m_Player.rigidBody.AddForce(
                Physics.gravity * (CurrentSkill.plungeFallGravityScale - 1f) * Time.deltaTime,
                ForceMode.VelocityChange);
        }
        else
        {
            ApplyGravityRatioWhenAttackAirborne();
        }
    }

    private void ApplyHorizontalControl()
    {
        if (m_IsPlunge)
            RotateWhenAttack();       
        else
            UpdateAirborneMovement();
    }

    private void HandleLanding()
    {   
        CurrentAttack.EndAbility();
    }

    #region Prediction
    private float PredictLandingTime()
    {
        float y0 = GetHeightAboveGround();        
        float vy0 = m_Player.verticalVelocity.y;
        float g = Physics.gravity.y * CurrentSkill.plungeFallGravityScale;
        // calculate y0 + vy0*t + 0.5*g*t² = 0, and get the positive result
        float disc = vy0 * vy0 - 2f * g * y0;
        if (disc < 0f) disc = 0f;
        float t = (-vy0 - Mathf.Sqrt(disc)) / g;
        return Mathf.Max(t, 0.1f);
    }

    private float GetHeightAboveGround()
    {
        CapsuleCollider capsule = m_Player.capsule;
        Vector3 origin = capsule.bounds.min + Vector3.up * 0.01f;        
        int mask = ~(1 << m_Player.gameObject.layer);

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 50f, mask))
            return Mathf.Max(0f, hit.distance);
        return 50f; 
    }   
    #endregion
}
