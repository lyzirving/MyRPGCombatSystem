using UnityEngine;

public class PlayerStateAttack : PlayerStateCombat
{    
    private float m_NormalizedTime = 0;

    private AttackAbility CurrentAttack => m_Player.abilitySystemComp.GetActive<AttackAbility>();

    public float CurrentNormalizedTime => m_NormalizedTime;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_NormalizedTime = 0f;
        m_Player.model.StartAnimation(m_Player.attackComponent.skill.animatorState, m_Player.attackComponent.skill.crossFadeInTime);        

        var ability = CurrentAttack;
        if(ability != null)
        {
            AnimationEventReceiver.instance.RegisterAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackCombo, ability.HandleAttackCombo);
            AnimationEventReceiver.instance.RegisterAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackVfxBegin, ability.HandleAttackVfxBegin);
            AnimationEventReceiver.instance.RegisterAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackVfxEnd, ability.HandleAttackVfxEnd);
        }
        else
        {
            Debug.LogError($"PlayerStateAttack::Enter() - no active AttackAbility found on player[{m_Player.name}]");
        }
    }

    public override void ReEnter(ChangeStateArgs args)
    {
        m_Player.model.StartAnimation(m_Player.attackComponent.skill.animatorState, m_Player.attackComponent.skill.crossFadeInTime);
        m_NormalizedTime = 0f;
    }

    public override bool Exit(StateBase newState)
    {
        m_Player.attackComponent.EndCombo();

        var ability = CurrentAttack;
        if (ability != null)
        {
            AnimationEventReceiver.instance.RemoveAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackCombo, ability.HandleAttackCombo);
            AnimationEventReceiver.instance.RemoveAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackVfxBegin, ability.HandleAttackVfxBegin);
            AnimationEventReceiver.instance.RemoveAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackVfxEnd, ability.HandleAttackVfxEnd);
        }
        else
        {
            Debug.LogError($"PlayerStateAttack::Exit() - no active AttackAbility found on player[{m_Player.name}]");
        }
        base.Exit(newState);
        return true;
    }

    public override void Update()
    {
        if(!IsExpired())
            m_Player.model.animator.GetTargetAnimationTime(m_Player.attackComponent.skill.animatorState, AnimationConsts.BASE_LAYER, out m_NormalizedTime);           
    }

    public override void FixedUpdate()
    {
        m_Player.ResetHorizontalVelocity();
        Vector3 targetDir;
        if (m_Player.lockTarget != null && m_Player.sensor.distZone.IsZone(EDistanceZone.CloseCombatRange))
        {
            targetDir = m_Player.lockTarget.transform.position - m_Player.transform.position;
            targetDir = targetDir.NormalizeIgnoreY();
        }
        else
        {
            targetDir = m_Player.GetTargetDirection();
        }
        m_Player.RotateToTargetDir(targetDir, m_Player.config.move.rotateSpeed);
    }

    public override bool IsExpired()
    {
        return m_NormalizedTime >= m_Player.attackComponent.skill.transitionNormalizedTime;
    }  

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Attack;
    }
}
