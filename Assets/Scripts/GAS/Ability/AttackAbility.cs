using UnityEngine;

public class AttackAbility : GameplayAbility
{
    public SkillData currentSkill => m_Character?.attackComponent?.skill;

    public float transitionNormalizedTime => currentSkill.transitionNormalizedTime; 

    /// <summary>
    /// Cached input when the player presses attack but combo window isn't open yet
    /// and the current attack cannot be interrupted.
    /// </summary>
    private CombatDefine.EAttack m_PendingComboInput = CombatDefine.EAttack.None;   

    /// <summary>
    /// When true, combo has been logically advanced (NextSkill called) but animation switch
    /// (ReActivate) is deferred until HitStop finishes.
    /// </summary>
    private bool m_DeferredReActivate = false;
    
    #region Ability API 
    public bool CanBeInterrupted(float currentNormalizedTime)
    {
        var skill = currentSkill;   
        return skill != null ? currentNormalizedTime >= skill.minInterruptNormalizedTime : true;           
    }
    
    protected override void OnAbilityActivated()
    {
        m_DeferredReActivate = false;
    }    

    protected override void OnAbilityCanceled()
    {
        m_PendingComboInput = CombatDefine.EAttack.None;
        m_DeferredReActivate = false;
        TransitionToLocomotion();        
    }

    protected override void OnAbilityEnded()
    {
        m_PendingComboInput = CombatDefine.EAttack.None;
        m_DeferredReActivate = false;
        TransitionToLocomotion();        
    }

    /// <summary>
    /// When the player is still holding movement input after attack ends,
    /// skip Idle and go directly to Move (or Sprint) to avoid animation blending
    /// through Idle, which causes visible sliding.
    /// LocomotionAbility will pick up the correct mode (Sprint/StrafeMove) 
    /// on the next OnAbilityUpdate.
    /// </summary>
    private void TransitionToLocomotion()
    {
        var player = m_Character as PlayerController;
        if (player != null && player.action.isMoving)
        {
            m_Character.ChangeState(ECharacterState.Move);
        }
        else
        {
            m_Character.ChangeState(ECharacterState.Idle);
        }
    }

    protected override void OnAbilityPerformed()
    {        
        m_Character.ChangeState(ECharacterState.Attack);
    }

    protected override void OnAbilityReEnter()
    {
        m_Character.ChangeState(ECharacterState.Attack);
    }

    protected override void OnAbilityUpdate(float deltaTime)
    {
        var state = m_Character.currentState as PlayerStateAttack;
        if (state == null)
        {
            EndAbility();
            return;
        }

        if (m_DeferredReActivate && !m_Character.model.isHitStopRunning)
        {
            m_DeferredReActivate = false;
            ReActivate(m_Character.abilitySystemComp);
        }

        if (state.IsExpired())
            EndAbility();
    }    
    #endregion

    #region  Pending input
    /// <summary>
    /// Cache a combo input when it can't be processed immediately.
    /// Returns true if the input was cached (not consumed).
    /// </summary>
    public bool CachePendingComboInput(CombatDefine.EAttack inputType)
    {
        if (m_PendingComboInput == CombatDefine.EAttack.None)
        {
            m_PendingComboInput = inputType;
            return true;
        }
        return false; // already has a pending input
    }

    /// <summary>
    /// Try to consume the pending combo input. Called when BeginCombo fires.
    /// Returns the consumed input type, or None if nothing pending.
    /// </summary>
    public CombatDefine.EAttack TryConsumePendingComboInput()
    {
        var pending = m_PendingComboInput;
        m_PendingComboInput = CombatDefine.EAttack.None;
        return pending;
    }

    public bool HasPendingComboInput()
    {
        return m_PendingComboInput != CombatDefine.EAttack.None;
    }
    #endregion
    
    #region Attack Event
    public void HandleAttackBegin(in AnimationEventInfo info)
    {
        m_Character?.OnAttackBegin();
    }

    public void HandleAttackEnd(in AnimationEventInfo info)
    {
        m_Character?.OnAttackEnd();
    }

    public void HandleAttackVfxBegin(in AnimationEventInfo info)
    {
        //[BugFix] fix animator graph doesn't sync with logic state
        if (info.animatorState != m_Character.attackComponent.skill.animatorState)
            return;

        m_Character.OnAttackVfxBegin();
    }

    public void HandleAttackVfxEnd(in AnimationEventInfo info)
    {
        //[BugFix] fix animator graph doesn't sync with logic state
        if (info.animatorState != m_Character.attackComponent.skill.animatorState)
            return;

        m_Character.OnAttackVfxEnd();
    }

    public void HandleAttackComboWindowOpened(in AnimationEventInfo info)
    {
        //[BugFix] fix animator graph doesn't sync with logic state
        if (info.animatorState != m_Character.attackComponent.skill.animatorState)
            return;

        m_Character.attackComponent.BeginCombo();
        if(HasPendingComboInput())
        {
            TryConsumePendingAndAdvance();
        }
    }

    public void HandleRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        Transform lockTarget = m_Character.lockTarget;
        SkillData skill = currentSkill;

        if (lockTarget != null && skill != null && skill.minDistanceToTarget > 0f)
        {
            // 1. Compute direction and current distance to target (y-axis ignored)
            Vector3 toTarget = lockTarget.position - m_Character.transform.position;
            toTarget.y = 0;
            float currentDist = toTarget.magnitude;
            if (currentDist < Mathf.Epsilon)
            {
                // Already exactly at target position — fall back to raw root motion
                m_Character.transform.Translate(deltaPosition, Space.World);
                return;
            }
            Vector3 toTargetDir = toTarget / currentDist;

            // 2. Decompose root motion into forward (toward target) and lateral components
            //    forwardMag > 0 = moving toward target (distance decreases)
            float forwardMag = Vector3.Dot(deltaPosition, toTargetDir);
            Vector3 lateralDelta = deltaPosition - forwardMag * toTargetDir;

            // 3. Soft clamp: prevent forward displacement from penetrating minDistance
            if (forwardMag > 0f)
            {
                float maxAllowedForward = currentDist - skill.minDistanceToTarget;
                float predictedDist = currentDist - forwardMag;
                if (predictedDist < skill.minDistanceToTarget)
                {
                    // Clamp forward to stop exactly at minDistance.
                    // If already inside the zone, maxAllowedForward is negative so forwardMag becomes 0.
                    forwardMag = Mathf.Max(0f, maxAllowedForward);
                }
            }

            // 4. Reconstruct and apply
            Vector3 finalDelta = toTargetDir * forwardMag + lateralDelta;
            m_Character.transform.Translate(finalDelta, Space.World);
        }
        else if(m_Character.softLockTarget != null && deltaPosition.sqrMagnitude > 0.001f)
        {
            // Soft-lock homing: subtle deflection toward nearest visible target (max 15°)
            ApplySoftLockHoming(deltaPosition);
        }
        else
        {
            m_Character.transform.Translate(deltaPosition, Space.World);
        }
    }

    /// <summary>
    /// Soft-lock root motion: homing toward target (max 22°) + minDistance clamp.
    /// Prevents the character from penetrating through the target during combos.
    /// </summary>
    private void ApplySoftLockHoming(Vector3 deltaPosition)
    {
        Transform softTarget = m_Character.softLockTarget;
        if (softTarget == null)
            return;

        Vector3 toTarget = softTarget.position - m_Character.transform.position;
        toTarget.y = 0;
        float currentDist = toTarget.magnitude;
        if (currentDist < 0.01f)
        {
            m_Character.transform.Translate(deltaPosition, Space.World);
            return;
        }
        Vector3 toTargetDir = toTarget.normalized;

        // 1. Homing: deflect root motion direction toward target (max 22°)
        Vector3 rootMotionDir = deltaPosition.normalized;
        float angleToTarget = Vector3.Angle(rootMotionDir, toTargetDir);
        const float maxHomingAngle = 22f;
        if (angleToTarget > 1f)
        {
            float homingAngle = Mathf.Min(angleToTarget, maxHomingAngle);
            rootMotionDir = Vector3.RotateTowards(
                rootMotionDir, toTargetDir, homingAngle * Mathf.Deg2Rad, 0f);
        }

        Vector3 homedDelta = rootMotionDir * deltaPosition.magnitude;

        // 2. minDistance clamp: prevent forward displacement from penetrating the target
        //    (same logic as hard lock)
        SkillData skill = currentSkill;
        if (skill != null && skill.minDistanceToTarget > 0f)
        {
            float forwardMag = Vector3.Dot(homedDelta, toTargetDir);
            Vector3 lateralDelta = homedDelta - forwardMag * toTargetDir;

            if (forwardMag > 0f)
            {
                float maxAllowedForward = currentDist - skill.minDistanceToTarget;
                float predictedDist = currentDist - forwardMag;
                if (predictedDist < skill.minDistanceToTarget)
                {
                    forwardMag = Mathf.Max(0f, maxAllowedForward);
                }
            }

            Vector3 finalDelta = toTargetDir * forwardMag + lateralDelta;
            m_Character.transform.Translate(finalDelta, Space.World);
        }
        else
        {
            m_Character.transform.Translate(homedDelta, Space.World);
        }
    }

    // <summary>
    /// if there's a pending input, try to advance combo with it.
    /// </summary>
    public void TryConsumePendingAndAdvance()
    {
        var pendingInput = TryConsumePendingComboInput();
        if (pendingInput != CombatDefine.EAttack.None)
        {
            if (m_Character.attackComponent.TryAdvanceCombo(pendingInput))
            {
                if (m_Character.model.isHitStopRunning)
                {
                    // HitStop is running：only advance logic, delay animation transition
                    m_DeferredReActivate = true;
                }
                else
                {
                    ReActivate(m_Character.abilitySystemComp);
                }
            }
            else
            {
                CachePendingComboInput(pendingInput);
            }
        }
    }
    #endregion
}