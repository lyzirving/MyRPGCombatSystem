
public class LightAttackAbility : GameplayAbility
{
    protected override void OnAbilityActivated()
    {       
    }    

    protected override void OnAbilityCanceled()
    {
        m_Character.ChangeState(ECharacterState.Idle);
    }

    protected override void OnAbilityEnded()
    {
        m_Character.ChangeState(ECharacterState.Idle);
    }

    protected override void OnAbilityPerformed()
    {
        m_Character.ChangeState(ECharacterState.Attack);
    }

    protected override void OnAbilityReEnter()
    {
        if (m_Character.attackComponent.GoNextSkill())
        {
            m_Character.attackComponent.NextSkill();
            m_Character.ChangeState(ECharacterState.Attack);
        }
    }

    protected override void OnAbilityUpdate(float deltaTime)
    {
        var state = m_Character.currentState as PlayerStateAttack;
        if (state == null)
        {
            EndAbility();
            return;
        }

        if (state.IsExpired())
            EndAbility();
    }
}
