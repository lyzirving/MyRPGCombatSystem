
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
        if (m_Character.lockTarget != null)
            m_Character.ChangeState(ECharacterState.StrafeMove);
        else
            m_Character.ChangeState(ECharacterState.Idle);
    }
}
