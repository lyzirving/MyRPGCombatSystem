using UnityEngine;

public class LockTargetAbility : GameplayAbility
{
    protected override void OnAbilityActivated()
    {
    }

    protected override void OnAbilityCanceled()
    {
        m_Character.model.SetAnimationBool(AnimationConsts.locked, false);
        if (m_Character.IsCurrentState<PlayerStateStrafeMove>())
            m_Character.ChangeState(ECharacterState.Move);
    }

    protected override void OnAbilityEnded()
    {
        m_Character.model.SetAnimationBool(AnimationConsts.locked, false);
        if (m_Character.IsCurrentState<PlayerStateStrafeMove>())
            m_Character.ChangeState(ECharacterState.Move);
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
    }
}
