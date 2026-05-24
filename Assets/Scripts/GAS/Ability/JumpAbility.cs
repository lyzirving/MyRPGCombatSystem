
public class JumpAbility : GameplayAbility
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
        m_Character.ChangeState(ECharacterState.Jump);
    }

    protected override void OnAbilityReEnter()
    {
    }

    protected override void OnAbilityUpdate(float deltaTime)
    {
        var state = m_Character.currentState as PlayerStateJump;
        if (state == null)
        {
            EndAbility();
            return;
        }

        if (state.IsExpired())
            EndAbility();
    }
}
