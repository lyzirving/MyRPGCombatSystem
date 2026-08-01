using UnityEngine;

public class AIStateAttack : AIStateCombat
{
    private float m_NormalizedTime = 0;
    private bool m_ComboWindowOpened = false;
    private EAttackState m_SubState;    

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);       
        m_AIController.model.StartAnimation(m_AIController.attackComponent.skill.animatorState, m_AIController.attackComponent.skill.crossFadeInTime);
        m_NormalizedTime = 0f;
        m_ComboWindowOpened = false;
        m_SubState = EAttackState.Start;
    }

    public override void ReEnter(ChangeStateArgs args)
    {
        if (m_SubState == EAttackState.ReadyCombo)
        {
            if (m_AIController.attackComponent.hasNextSkill)
            {
                m_AIController.attackComponent.NextSkill();
                // trigger combat sequence                
                m_AIController.model.StartAnimation(m_AIController.attackComponent.skill.animatorState, m_AIController.attackComponent.skill.crossFadeInTime);
                m_NormalizedTime = 0f;
                m_ComboWindowOpened = false;
                m_SubState = EAttackState.Start;
            }
        }        
    }

    public override void Update()
    {
        m_AIController.model.animator.GetTargetAnimationTime(m_AIController.attackComponent.skill.animatorState, AnimationConsts.BASE_LAYER, out m_NormalizedTime);

        var skill = m_AIController.attackComponent.skill;
        if (!m_ComboWindowOpened && m_NormalizedTime >= skill.comboWindowStartNormalizedTime)
        {
            m_ComboWindowOpened = true;
            m_AIController.attackComponent.BeginCombo();
            HandleAttackCombo();
        }

        if (m_NormalizedTime >= m_AIController.attackComponent.skill.transitionNormalizedTime)
        {
            m_SubState = EAttackState.End;
            m_AIController.attackComponent.EndCombo();
            m_AIController.ChangeState(ECharacterState.Idle);
            return;
        }
    }

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Attack;
    }

    private void HandleAttackCombo()
    {        
        m_SubState = EAttackState.ReadyCombo;        
        m_AIController.ChangeState(ECharacterState.Attack);
    }
}
