using UnityEngine;

/// <summary>
/// Manages the lock-target combat stance.
/// Sole authority over whether the character is in "locked-on" state (Tag.Locked).
/// Does NOT directly change movement states — that is LocomotionAbility's job,
/// which reads Tag.Locked to decide whether to enter StrafeMove.
/// </summary>
public class LockTargetAbility : GameplayAbility
{
    private PlayerController m_Player;

    protected override void OnAbilityActivated()
    {
        m_Player = m_Character as PlayerController;
    }

    protected override void OnAbilityPerformed()
    {
        m_Character.model.SetAnimationBool(AnimationConsts.locked, true);
    }

    protected override void OnAbilityReEnter()
    {
        m_Character.model.SetAnimationBool(AnimationConsts.locked, true);
    }

    protected override void OnAbilityUpdate(float deltaTime)
    {
        if (m_Player == null)
        {
            EndAbility();
            return;
        }

        // If the player looks away from the locked target, release the lock.
        if (m_Player.lockTarget != null && !m_Player.sensor.WithinView(m_Player.action.cameraFwd))
        {
            m_Player.lockTarget = null;
            EndAbility();
            return;
        }

        // If the lock target no longer exists, end the ability.
        if (m_Player.lockTarget == null)
        {
            EndAbility();
        }
    }

    protected override void OnAbilityEnded()
    {
        // LocomotionAbility will detect Tag.Locked removal and auto-transition
        // from StrafeMove to Move/Sprint in its next OnAbilityUpdate.
        m_Character.model.SetAnimationBool(AnimationConsts.locked, false);        
    }

    protected override void OnAbilityCanceled()
    {
        // LocomotionAbility will detect Tag.Locked removal and auto-transition
        // from StrafeMove to Move/Sprint in its next OnAbilityUpdate.
        m_Character.model.SetAnimationBool(AnimationConsts.locked, false);        
    }
}
