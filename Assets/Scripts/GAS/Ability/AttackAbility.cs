using UnityEngine;

public class AttackAbility : GameplayAbility
{
    public virtual CombatDefine.EAttack AttackActionType => CombatDefine.EAttack.LA;
    /// <summary>
    /// Index of AttackComponent's comboSequences array
    /// </summary>
    protected int m_ComboIndex = 0;

    public SkillData currentSkill => m_Character?.attackComponent?.skill;

    public float transitionNormalizedTime => currentSkill.transitionNormalizedTime;    
    
    #region Ability API    
    public bool CanBeInterrupted(float currentNormalizedTime)
    {
        var skill = currentSkill;   
        return skill != null ? currentNormalizedTime >= skill.minInterruptNormalizedTime : true;           
    }
    
    protected override void OnAbilityActivated()
    {
        m_Character.attackComponent.SetComboIndex(m_ComboIndex);
    }    

    protected override void OnAbilityCanceled()
    {
        m_Character.ChangeState(ECharacterState.Idle);
    }

    protected override void OnAbilityEnded()
    {
        m_Character.ChangeState(ECharacterState.Idle);
    }

    protected override void OnAbilityPerformed()
    {        
        m_Character.ChangeState(ECharacterState.Attack);
    }

    protected override void OnAbilityReEnter()
    {
        if (m_Character.attackComponent.CanAdvanceNextSkill(AttackActionType))
        {
            m_Character.attackComponent.NextSkill();
            m_Character.ChangeState(ECharacterState.Attack);
        }
        else
        {
            EndAbility();
        }
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
    #endregion
    
    #region Attack Event
    public virtual void HandleAttackCombo(in AnimationEventInfo info)
    {
        //[BugFix] fix animator graph doesn't sync with logic state
        if (info.animatorState != m_Character.attackComponent.skill.animatorState)
            return;

        m_Character.attackComponent.BeginCombo();
    }

    public virtual void HandleAttackVfxBegin(in AnimationEventInfo info)
    {
        //[BugFix] fix animator graph doesn't sync with logic state
        if (info.animatorState != m_Character.attackComponent.skill.animatorState)
            return;

        m_Character.OnAttackVfxBegin();
    }

    public virtual  void HandleAttackVfxEnd(in AnimationEventInfo info)
    {
        //[BugFix] fix animator graph doesn't sync with logic state
        if (info.animatorState != m_Character.attackComponent.skill.animatorState)
            return;

        m_Character.OnAttackVfxEnd();
    }
    #endregion
}
