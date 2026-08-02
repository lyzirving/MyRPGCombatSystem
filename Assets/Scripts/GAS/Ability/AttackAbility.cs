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
        m_Character.ChangeState(ECharacterState.Idle);
        m_PendingComboInput = CombatDefine.EAttack.None;
        m_DeferredReActivate = false;
    }

    protected override void OnAbilityEnded()
    {
        m_Character.ChangeState(ECharacterState.Idle);
        m_PendingComboInput = CombatDefine.EAttack.None;
        m_DeferredReActivate = false;
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

    public void HandleComboWindowOpened()
    {
        TryConsumePendingAndAdvance();
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

    public void HandleRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        m_Character.transform.Translate(deltaPosition, Space.World);
    }

    // <summary>
    /// if there's a pending input, try to advance combo with it.
    /// </summary>
    private void TryConsumePendingAndAdvance()
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
