
public class AirborneAttackAbility : AttackAbility
{
    protected override void OnAbilityCanceled()
    {
        m_PendingComboInput = CombatDefine.EAttack.None;
        m_DeferredReActivate = false;
        DowngradeLockAfterAttack();
        TransitionToOtherState();
    }    

    protected override void OnAbilityEnded()
    {
        m_PendingComboInput = CombatDefine.EAttack.None;
        m_DeferredReActivate = false;
        DowngradeLockAfterAttack();
        TransitionToOtherState();
    }

    protected override void OnAbilityPerformed()
    {
        m_Character.ChangeState(ECharacterState.AirborneAttack);
    }

    protected override void OnAbilityReEnter()
    {
        m_Character.ChangeState(ECharacterState.AirborneAttack);
    }

    protected override void OnAbilityUpdate(float deltaTime)
    {
        if (m_Character.currentState is not PlayerStateAirborneAttack state)
        {
            EndAbility();
            return;
        }

        if (m_DeferredReActivate && !m_Character.model.isHitStopRunning)
        {
            m_DeferredReActivate = false;
            ReActivate(m_Character.abilitySystemComp);
        }

        if (state.IsExpired())
        {
            EndAbility();
        }
    }

    public override void HandleAttackBegin(in AnimationEventInfo info)
    {        
        (m_Character as PlayerController)?.ExhaustAirAttack();
        base.HandleAttackBegin(info);
    }

    private void TransitionToOtherState()
    {
        if(m_Character.sensor.isGrounded)
        {
            m_Character.ChangeState(ECharacterState.Idle);
            return;
        }
        var step = (m_Character.currentState as PlayerStateAirborneAttack)?.CurrentFootstep ?? EFootstep.None;
        m_Character.ChangeState(ECharacterState.Falling, new ChangeStateArgs{ footStep = step});
    }
}