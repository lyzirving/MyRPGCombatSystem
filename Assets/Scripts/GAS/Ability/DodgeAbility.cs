
/// <summary>
/// DodgeAbility is a simple ability that triggers a dodge action on the player character.
/// It does not handle movement or state transitions directly; instead, 
/// it relies on the PlayerController to perform the dodge action and change the character's state to Dodge.
/// It requires Tag.locked to be activated to ensure that the player is in a state where dodging is allowed.
/// </summary>
public class DodgeAbility : GameplayAbility
{
    protected override void OnAbilityActivated()
    {
    }

    protected override void OnAbilityCanceled()
    {
        ChangeStateWhenExit();
    }

    protected override void OnAbilityEnded()
    {
        ChangeStateWhenExit();
    }

    protected override void OnAbilityPerformed()
    {
        var player = m_Character as PlayerController;
        if (player != null)
            player.MakeDodgeAction(player.action.playerMovement);
        m_Character.ChangeState(ECharacterState.Dodge);
    }

    protected override void OnAbilityReEnter()
    {
    }

    protected override void OnAbilityUpdate(float deltaTime)
    {
        var state = m_Character.currentState as PlayerStateDodge;
        if (state == null)
        {
            EndAbility();
            return;
        }

        if (state.IsExpired())
            EndAbility();
    }

    private void ChangeStateWhenExit()
    {
        // Always return to Idle. If the player is still holding movement input,
        // LocomotionAbility will be re-activated next frame and decide the correct
        // locomotion mode (Move / StrafeMove / Sprint) based on current Tags and input.
        m_Character.ChangeState(ECharacterState.Idle);
    }
}
