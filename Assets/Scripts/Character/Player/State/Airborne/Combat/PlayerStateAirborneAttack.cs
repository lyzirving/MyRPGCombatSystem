
using UnityEngine;

public class PlayerStateAirborneAttack : PlayerStateAirborneCombat
{
    private float m_NormalizedTime = 0;

    private AirborneAttackAbility CurrentAttack => m_Player.abilitySystemComp.GetActive<AirborneAttackAbility>();

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
        }
        else
        {
            Debug.LogError($"PlayerStateAirborneAttack::Enter() - no active AirborneAttackAbility found on player[{m_Player.name}]");
        }
    }

    public override void ReEnter(ChangeStateArgs args)
    {        
        m_Player.ResetAirAttack();
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
        }
        else
        {
            Debug.LogError($"PlayerStateAirborneAttack::Exit() - no active AttackAbility found on player[{m_Player.name}]");
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
        // Poll grounded state every fixed frame instead of relying only on the touch event:
        // the touch callback only fires when the grounded flag *changes*, so a transition that
        // happens while already grounded (e.g. the jump anti-stuck timeout) would never land.
        if (m_Player.sensor.isGrounded)
        {
            CurrentAttack.EndAbility();
            return;
        }

        ApplyGravityRatioWhenAttackAirborne();

        RotateWhenAttack();

        UpdateAirborneMovement();
    }

    public override bool IsExpired()
    {
        return m_NormalizedTime >= m_Player.attackComponent.skill.transitionNormalizedTime;
    }

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Attack;
    }

    public override void OnContactGround(Collider collider)
    {
        m_Player.OnFootStep(EFootstep.None);
        CurrentAttack.EndAbility();
    }
}
