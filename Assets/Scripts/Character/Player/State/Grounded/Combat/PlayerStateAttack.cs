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

        // Soft-lock snap: instantly face toward soft-lock target before attack (max 30°)
        SnapToSoftLockTarget();

        m_Player.model.StartAnimation(m_Player.attackComponent.skill.animatorState, m_Player.attackComponent.skill.crossFadeInTime);        

        var ability = CurrentAttack;
        if(ability != null)
        {
            AnimationEventReceiver.instance.RegisterAction(m_Player.model.animator, AnimationEventType.AttackVfxBegin, ability.HandleAttackVfxBegin);
            AnimationEventReceiver.instance.RegisterAction(m_Player.model.animator, AnimationEventType.AttackVfxEnd, ability.HandleAttackVfxEnd);
            AnimationEventReceiver.instance.RegisterAction(m_Player.model.animator, AnimationEventType.AttackStart, ability.HandleAttackBegin);
            AnimationEventReceiver.instance.RegisterAction(m_Player.model.animator, AnimationEventType.AttackEnd, ability.HandleAttackEnd); 
            AnimationEventReceiver.instance.RegisterAction(m_Player.model.animator, AnimationEventType.AttackComboWindowOpened, ability.HandleAttackComboWindowOpened);
            m_Player.model.RegisterRootMotionAction(ability.HandleRootMotion);
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
            AnimationEventReceiver.instance.RemoveAction(m_Player.model.animator, AnimationEventType.AttackVfxBegin, ability.HandleAttackVfxBegin);
            AnimationEventReceiver.instance.RemoveAction(m_Player.model.animator, AnimationEventType.AttackVfxEnd, ability.HandleAttackVfxEnd);
            AnimationEventReceiver.instance.RemoveAction(m_Player.model.animator, AnimationEventType.AttackStart, ability.HandleAttackBegin);
            AnimationEventReceiver.instance.RemoveAction(m_Player.model.animator, AnimationEventType.AttackEnd, ability.HandleAttackEnd);
            AnimationEventReceiver.instance.RemoveAction(m_Player.model.animator, AnimationEventType.AttackComboWindowOpened, ability.HandleAttackComboWindowOpened);
            m_Player.model.RemoveRootMotionAction(ability.HandleRootMotion);
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
        {
            m_Player.model.animator.GetTargetAnimationTime(m_Player.attackComponent.skill.animatorState, AnimationConsts.BASE_LAYER, out m_NormalizedTime);
        }        
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

            // Soft lock: continuously blend facing toward soft-lock target (30%)
            // so Enter()'s initial snap isn't dragged back to pure input direction.
            Transform softTarget = m_Player.softLockTarget;
            if (softTarget != null)
            {
                Vector3 toTarget = softTarget.position - m_Player.transform.position;
                toTarget.y = 0;
                if (toTarget.sqrMagnitude > 0.01f)
                {
                    targetDir = Vector3.Slerp(targetDir, toTarget.normalized, 0.3f).normalized;
                }
            }
        }
        m_Player.RotateToTargetDir(targetDir, m_Player.config.move.rotateSpeed);
    }

    public override bool IsExpired()
    {
        return m_NormalizedTime >= m_Player.attackComponent.skill.transitionNormalizedTime;
    }

    /// <summary>
    /// Before the attack animation starts, instantly snap rotation toward
    /// the soft-lock target (up to 30° correction). If the target is beyond
    /// 30°, attack in the original facing direction.
    /// Does NOT affect movement — only rotation.
    /// </summary>
    private void SnapToSoftLockTarget()
    {
        // Hard lock already handles facing in FixedUpdate; only snap for soft lock
        if (m_Player.lockTarget != null)
            return;

        Transform softTarget = m_Player.softLockTarget;
        if (softTarget == null)
            return;

        Vector3 toTarget = softTarget.position - m_Player.transform.position;
        toTarget.y = 0;
        if (toTarget.sqrMagnitude < 0.01f)
            return;

        Vector3 targetDir = toTarget.normalized;
        float angle = Vector3.Angle(m_Player.transform.forward, targetDir);

        // Max 30° correction; beyond that, attack in original direction
        const float maxSnapAngle = 30f;
        if (angle > maxSnapAngle)
            return;

        // Smooth rotation (not instant LookRotation) — uses high rotate speed
        m_Player.RotateToTargetDir(targetDir, m_Player.config.move.rotateSpeed * 3f);
    }

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Attack;
    }    
}
