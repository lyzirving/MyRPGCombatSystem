
public class DefenceAbility : GameplayAbility
{
    protected override void OnAbilityActivated()
    {
    }

    protected override void OnAbilityCanceled()
    {
        m_Character.ChangeToLocomotionState();
    }

    protected override void OnAbilityEnded()
    {
        m_Character.ChangeToLocomotionState();
    }    

    protected override void OnAbilityPerformed()
    {
        m_Character.ChangeState(ECharacterState.Defence);
    }

    protected override void OnAbilityReEnter()
    {
    }

    protected override void OnAbilityUpdate(float deltaTime)
    {
        if (m_Character.currentState is not PlayerStateDefence state)
        {
            EndAbility();
            return;
        }

        if (state.IsExpired())
            EndAbility();
    }
}
