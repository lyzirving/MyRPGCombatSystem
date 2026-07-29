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
    
    #region Ability API 
    public bool CanBeInterrupted(float currentNormalizedTime)
    {
        var skill = currentSkill;   
        return skill != null ? currentNormalizedTime >= skill.minInterruptNormalizedTime : true;           
    }
    
    protected override void OnAbilityActivated()
    {
    }    

    protected override void OnAbilityCanceled()
    {
        m_Character.ChangeState(ECharacterState.Idle);
        m_PendingComboInput = CombatDefine.EAttack.None;
    }

    protected override void OnAbilityEnded()
    {
        m_Character.ChangeState(ECharacterState.Idle);
        m_PendingComboInput = CombatDefine.EAttack.None;
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

        if (state.IsExpired())
            EndAbility();
    }

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
    public void HandleAttackCombo(in AnimationEventInfo info)
    {
        //[BugFix] fix animator graph doesn't sync with logic state
        if (info.animatorState != m_Character.attackComponent.skill.animatorState)
            return;

        m_Character.attackComponent.BeginCombo();

        var pendingInput = TryConsumePendingComboInput();
        if (pendingInput != CombatDefine.EAttack.None)
        {
            // Try to advance combo with the cached input
            if (m_Character.attackComponent.TryAdvanceCombo(pendingInput))
            {
                // Success: re-enter the attack state with the next skill
                ReActivate(m_Character.abilitySystemComp);
            }
            // If TryAdvanceCombo fails, the pending input cannot be consumed now.
            // It was already cleared by TryConsumePendingComboInput, which is acceptable
            // because the input might have expired between cache and now.
        }
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
    #endregion
}
