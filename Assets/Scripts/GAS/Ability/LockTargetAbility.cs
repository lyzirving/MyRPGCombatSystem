using UnityEngine;

/// <summary>
/// Passive marker ability for hard lock-on state.
/// 
/// Sole responsibility: manage Tag.Locked (via grantedTags) and the
/// "locked" animation parameter. All lock-target validation, target
/// selection, and input handling is done by LockTargetManager.
/// 
/// LocomotionAbility reads Tag.Locked to decide whether to enter StrafeMove.
/// </summary>
public class LockTargetAbility : GameplayAbility
{
    protected override void OnAbilityActivated()
    {
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
        // Passive — all lifecycle management is external (LockTargetManager).
    }

    protected override void OnAbilityEnded()
    {
        // Tag.Locked (grantedTags) is auto-removed by base class.
        // LocomotionAbility will detect the removal and transition out of StrafeMove.
        m_Character.model.SetAnimationBool(AnimationConsts.locked, false);
    }

    protected override void OnAbilityCanceled()
    {
        m_Character.model.SetAnimationBool(AnimationConsts.locked, false);
    }
}
