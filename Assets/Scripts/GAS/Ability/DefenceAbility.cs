
public class DefenceAbility : GameplayAbility
{
    protected override void OnAbilityActivated()
    {
    }

    protected override void OnAbilityCanceled()
    {
        TransitionToLocomotion();
    }

    protected override void OnAbilityEnded()
    {
        TransitionToLocomotion();
    }

    /// <summary>
    /// When the player is still holding movement input after defence ends,
    /// skip Idle and go directly to Move to avoid animation blending
    /// through Idle, which causes visible sliding.
    /// </summary>
    private void TransitionToLocomotion()
    {
        var player = m_Character as PlayerController;
        if (player != null && player.action.isMoving)
        {
            m_Character.ChangeState(ECharacterState.Move);
        }
        else
        {
            m_Character.ChangeState(ECharacterState.Idle);
        }
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
        var state = m_Character.currentState as PlayerStateDefence;
        if (state == null)
        {
            EndAbility();
            return;
        }

        if (state.IsExpired())
            EndAbility();
    }
}
