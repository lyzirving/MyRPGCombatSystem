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
        RotateWhenAttack();
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
