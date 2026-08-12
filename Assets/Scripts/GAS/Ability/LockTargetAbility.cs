using UnityEngine;

/// <summary>
/// Marker ability for lock-on state (hard or soft).
/// 
/// ScriptableObject config:
///   grantedTags: [Combat.Locked]    ← base tag, auto managed by GAS
///   blockedTags: [Locomotion.Sprint, ...]
/// 
/// Runtime sub-tags (managed via public methods):
///   Combat.Locked.Hard  ← SwitchToHardLock()  → LocomotionAbility → StrafeMove
///   Combat.Locked.Soft  ← SwitchToSoftLock()  → attack homing only
/// 
/// All tag management is self-contained in the ability lifecycle.
/// LockTargetManager calls SwitchToHardLock/Soft/None without any GC allocation.
/// </summary>
public class LockTargetAbility : GameplayAbility
{
    private enum LockMode { None, Hard, Soft }

    /// <summary>
    /// True if the ability is active in the specified mode.
    /// </summary>
    public bool IsInHardMode => m_Mode == LockMode.Hard && isActive;
    public bool IsInSoftMode => m_Mode == LockMode.Soft && isActive;

    private LockMode m_Mode = LockMode.None;

    private GameplayTag m_TagHard;
    private GameplayTag m_TagSoft;

    protected override void OnAbilityActivated()
    {
        m_TagHard = GameplayTag.CreateTag(GameplayTagManager.instance.GetIndex(GameplayTag.COMBAT_LOCKED_HARD));
        m_TagSoft = GameplayTag.CreateTag(GameplayTagManager.instance.GetIndex(GameplayTag.COMBAT_LOCKED_SOFT));
    }

    protected override void OnAbilityPerformed()
    {
        // Mode is set by caller after activation (SwitchToHardLock/SoftLock).
        // Animation is applied regardless of mode.
        m_Character.model.SetAnimationBool(AnimationConsts.locked, true);
    }

    protected override void OnAbilityReEnter()
    {
        // ReActivate called on target switch — keep current mode, re-apply animation
        m_Character.model.SetAnimationBool(AnimationConsts.locked, true);
    }

    protected override void OnAbilityUpdate(float deltaTime) { }

    protected override void OnAbilityEnded()
    {
        ExitMode();
        m_Character.model.SetAnimationBool(AnimationConsts.locked, false);
    }

    protected override void OnAbilityCanceled()
    {
        ExitMode();
        m_Character.model.SetAnimationBool(AnimationConsts.locked, false);
    }

    public void SwitchToHardLock()
    {
        if (m_Mode == LockMode.Hard) return;
        ApplyMode(LockMode.Hard);
    }

    public void SwitchToSoftLock()
    {
        if (m_Mode == LockMode.Soft) return;
        ApplyMode(LockMode.Soft);
    }

    public void SwitchToNone()
    {
        if (m_Mode == LockMode.None) return;
        ApplyMode(LockMode.None);
    }    

    // ──── Internal ────
    void ApplyMode(LockMode newMode)
    {
        ExitMode();
        EnterMode(newMode);
    }

    void EnterMode(LockMode mode)
    {
        m_Mode = mode;
        if (mode == LockMode.Hard)
            m_ASC.AddTag(m_TagHard);
        else if (mode == LockMode.Soft)
            m_ASC.AddTag(m_TagSoft);
    }

    void ExitMode()
    {
        if (m_Mode == LockMode.Hard)
            m_ASC.RemoveTag(m_TagHard);
        else if (m_Mode == LockMode.Soft)
            m_ASC.RemoveTag(m_TagSoft);
        m_Mode = LockMode.None;
    }    
}
