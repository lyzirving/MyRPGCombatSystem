using UnityEngine;

public class PlayerStateAttack : PlayerStateCombat
{
    private float m_NormalizedTime = 0;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.attackComponent.skill.animatorState, m_Player.attackComponent.skill.crossFadeInTime);
        m_NormalizedTime = 0f;
        AnimationEventReceiver.instance.RegisterAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackCombo, HandleAttackCombo);        
    }

    public override void ReEnter(ChangeStateArgs args)
    {
        m_Player.model.StartAnimation(m_Player.attackComponent.skill.animatorState, m_Player.attackComponent.skill.crossFadeInTime);
        m_NormalizedTime = 0f;
    }

    public override bool Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackCombo, HandleAttackCombo);
        base.Exit(newState);
        return true;
    }

    public override void Update()
    {
        if (!m_Player.model.animator.GetTargetAnimationTime(m_Player.attackComponent.skill.animatorState, AnimationConsts.BASE_LAYER, out m_NormalizedTime))
        {
            Debug.LogError($"Fail to get animator state[{m_Player.attackComponent.skill.animatorState}]'s normalized time");
            return;
        }

        // Break attack and turn into defence
        if (m_Player.action.holdDefence)
        {
            m_Player.attackComponent.EndCombo();
            m_Player.ChangeState(ECharacterState.Defence, new ChangeStateArgs(ChangeStateArgs.EAnimationPlayMode.Manual));
            return;
        }

        if (m_Player.attackComponent.UpdateCombo())
        {
            m_Player.attackComponent.NextSkill();
            m_Player.ChangeState(ECharacterState.Attack);
            return;
        }

        if (m_NormalizedTime >= m_Player.attackComponent.skill.transitionNormalizedTime)
        {
            m_Player.attackComponent.EndCombo();
            m_Player.ChangeState(ECharacterState.Idle);
            return;
        }

        // Change to another state
        if (m_Player.action.isJump)
        {
            m_Player.attackComponent.EndCombo();
            m_Player.ChangeState(ECharacterState.Jump);                        
        }              
    }

    public override void FixedUpdate()
    {
        m_Player.ResetHorizontalVelocity();

        if (!m_Player.action.isMoving)
            return;

        Vector3 targetDir = m_Player.GetTargetDirection();
        m_Player.RotateToTargetDir(targetDir, m_Player.config.rotateSpeed);
    }

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Attack;
    }

    private void HandleAttackCombo(in AnimationEventInfo info)
    {
        //[BugFix] fix animator graph doesn't sync with logic state
        if (info.animatorState != m_Player.attackComponent.skill.animatorState)
            return;

        m_Player.attackComponent.BeginCombo();
    }
}
