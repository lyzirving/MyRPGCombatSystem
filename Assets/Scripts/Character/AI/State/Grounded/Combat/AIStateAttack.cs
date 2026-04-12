using UnityEngine;

public class AIStateAttack : AIStateCombat
{
    private float m_NormalizedTime = 0;
    private EAttackState m_SubState;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);       
        m_AIController.model.StartAnimation(m_AIController.attackComponent.skill.animatorState, m_AIController.attackComponent.skill.crossFadeInTime);
        m_NormalizedTime = 0f;
        m_SubState = EAttackState.Start;
        AnimationEventReceiver.instance.RegisterAction(GUIDConsts.AIAnimation, AnimationEventType.AttackCombo, HandleAttackCombo);
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
                m_SubState = EAttackState.Start;
            }
        }        
    }

    public override bool Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveAction(GUIDConsts.AIAnimation, AnimationEventType.AttackCombo, HandleAttackCombo);
        base.Exit(newState);
        return true;
    }

    public override void Update()
    {
        m_AIController.model.animator.GetTargetAnimationTime(m_AIController.attackComponent.skill.animatorState, AnimationConsts.BASE_LAYER, out m_NormalizedTime);

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

    private void HandleAttackCombo(in AnimationEventInfo info)
    {
        //[BugFix] fix animator graph doesn't sync with logic state
        if (info.animatorState != m_AIController.attackComponent.skill.animatorState)
            return;
        //Debug.LogError($"AIStateAttack HandleAttackCombo, current state[{info.animatorState}]");
        m_SubState = EAttackState.ReadyCombo;        
        m_AIController.ChangeState(ECharacterState.Attack);
    }
}
