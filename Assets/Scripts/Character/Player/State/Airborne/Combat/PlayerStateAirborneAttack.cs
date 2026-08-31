
using UnityEngine;

public class PlayerStateAirborneAttack : PlayerStateAirborneCombat
{
    private float m_NormalizedTime = 0;
    private float m_Footstep = 0f;

    public virtual EFootstep CurrentFootstep
    {
        get 
        {
            return Mathf.Approximately(m_Footstep, 0f) ? EFootstep.None : (m_Footstep > 0f ? EFootstep.LeftFootstep : EFootstep.RightFootstep);
        }
    }

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
        if (ability != null)
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
        if (!IsExpired())
        {
            m_Player.model.animator.GetTargetAnimationTime(m_Player.attackComponent.skill.animatorState, AnimationConsts.BASE_LAYER, out m_NormalizedTime);
            SampleFootstep();
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

    /// <summary>
    /// Sample the "footstep" curve from the current air-attack clip.
    /// The curve is bound to PlayerModel.footstep and driven automatically by the Animator,
    /// so reading the field at this moment is the sample value.
    /// </summary>
    private void SampleFootstep()
    {
        // Default: reset before sampling so stale values never leak into m_Footstep.
        m_Footstep = 0f;

        var animator = m_Player.model.animator;
        if (animator == null)
            return;

        int layer = AnimationConsts.BASE_LAYER;

        // Match GetTargetAnimationTime's behaviour: during a transition, sample the *next* clip,
        // otherwise sample the current clip.
        AnimatorClipInfo[] clipInfos = animator.IsInTransition(layer)
            ? animator.GetNextAnimatorClipInfo(layer)
            : animator.GetCurrentAnimatorClipInfo(layer);

        if (clipInfos == null || clipInfos.Length == 0)
            return;

        AnimationClip clip = clipInfos[0].clip;
        if (clip == null)
            return;

        // The curve drives PlayerModel.footstep automatically; just read the current value.
        m_Footstep = (m_Player.model as PlayerModel)?.footstep ?? 0;
        //Debug.Log($"foot step {m_Footstep}");
    }
}
