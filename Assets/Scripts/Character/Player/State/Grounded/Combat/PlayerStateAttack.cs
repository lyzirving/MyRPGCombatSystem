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
        m_Player.model.animator.GetTargetAnimationTime(m_Player.attackComponent.skill.animatorState, AnimationConsts.BASE_LAYER, out m_NormalizedTime);
        if (m_NormalizedTime >= m_Player.attackComponent.skill.transitionNormalizedTime)
        {
            m_Player.attackComponent.EndCombo();
            Execute(ECharacterAction.Idle);
            return;
        }             
    }

    public override void FixedUpdate()
    {
        m_Player.ResetHorizontalVelocity();

        if (!m_Player.action.isMoving || m_Player.model.GetAnimationBool(AnimationConsts.locked))
            return;

        Vector3 targetDir = m_Player.GetTargetDirection();
        m_Player.RotateToTargetDir(targetDir, m_Player.config.move.rotateSpeed);
    }

    public override bool CanExecute(ECharacterAction action)
    {
        switch (action)
        {
            case ECharacterAction.Defence:
            case ECharacterAction.LightAttack:
            case ECharacterAction.Jump:
                return true;
            default:
                return false;
        }
    }

    public override void Execute(ECharacterAction action)
    {
        switch (action)
        {
            case ECharacterAction.Idle:
                m_Player.ChangeState(ECharacterState.Idle);
                return;
            case ECharacterAction.Defence:
                m_Player.attackComponent.EndCombo();
                m_Player.ChangeState(ECharacterState.Defence, new ChangeStateArgs(ChangeStateArgs.EAnimationPlayMode.Manual));
                return;
            case ECharacterAction.Jump:
                m_Player.attackComponent.EndCombo();
                m_Player.ChangeState(ECharacterState.Jump);
                return;
            case ECharacterAction.LightAttack:
                if (m_Player.attackComponent.GoNextSkill())
                {
                    m_Player.attackComponent.NextSkill();
                    m_Player.ChangeState(ECharacterState.Attack);
                }
                return;
            default: 
                break;
        }
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
