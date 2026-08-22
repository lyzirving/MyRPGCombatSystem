
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
        // Re-activation while airborne is the double jump.
        if(m_Character.currentState is PlayerStateAirborne stateAirborne)
            stateAirborne.TryDoubleJump();
    }

    protected override void OnAbilityUpdate(float deltaTime)
    {
        // Keep the ability active while airborne (jump or fall) so a re-activation can trigger
        // the double jump. It only ends once the character has left both airborne states (landed).
        if(!(m_Character.currentState is PlayerStateAirborne))
        {
            EndAbility();
            return;
        }

        if(m_Character.currentState is PlayerStateJump stateJump && stateJump.IsExpired())
            EndAbility();
    }
}
