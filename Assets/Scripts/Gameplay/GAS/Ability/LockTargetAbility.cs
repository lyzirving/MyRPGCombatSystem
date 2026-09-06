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
    [Header("LookAt IK Attributes")]
    [SerializeField] private float m_LookAtMaxAngle = 50f;

    [SerializeField] private float m_LookAtMaxWeight = 0.8f;
    
    [SerializeField] private float m_LookAtSmoothSpeed = 8f;

    [SerializeField] private float m_LookAtTargetHeight = 1.5f;

    private enum LockMode { None, Hard, Soft }

    /// <summary>
    /// True if the ability is active in the specified mode.
    /// </summary>
    public bool IsInHardMode => m_Mode == LockMode.Hard && isActive;
    public bool IsInSoftMode => m_Mode == LockMode.Soft && isActive;    

    private LockMode m_Mode = LockMode.None;

    private GameplayTag m_TagHard;
    private GameplayTag m_TagSoft;

    private Vector3 m_CurrentLookAtPos;
    private float m_CurrentLookWeight;

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
        m_Character.model.RegisterIKAction(OnPlayerIK);
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
        m_Character.model.RemoveIKAction(OnPlayerIK);
    }

    protected override void OnAbilityCanceled()
    {
        ExitMode();
        m_Character.model.SetAnimationBool(AnimationConsts.locked, false);
        m_Character.model.RemoveIKAction(OnPlayerIK);
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

    private void OnPlayerIK(int layerIndex)
    {
        if(m_ASC.HasTag(m_TagSoft))
            LookAtTargetIK(layerIndex);
        else
            m_Character.model.animator.SetLookAtWeight(0f);
    }

    // ──── Internal ────
    private void ApplyMode(LockMode newMode)
    {
        ExitMode();
        EnterMode(newMode);
    }

    private void EnterMode(LockMode mode)
    {
        m_Mode = mode;
        if (mode == LockMode.Hard)
            m_ASC.AddTag(m_TagHard);
        else if (mode == LockMode.Soft)
            m_ASC.AddTag(m_TagSoft);
    }

    private void ExitMode()
    {
        if (m_Mode == LockMode.Hard)
            m_ASC.RemoveTag(m_TagHard);
        else if (m_Mode == LockMode.Soft)
            m_ASC.RemoveTag(m_TagSoft);
        m_Mode = LockMode.None;
    }    

    private void LookAtTargetIK(int layerIndex)
    {
        if (layerIndex != AnimationConsts.BASE_LAYER)
            return;

        float targetWeight = 0f;
        var target = m_Character.softLockTarget;

        if (target != null)
        {            
            Vector3 toTarget = target.position - m_Character.transform.position;
            toTarget.y = 0;

            if (toTarget.sqrMagnitude > 0.01f)
            {
                float angle = Vector3.Angle(m_Character.transform.forward, toTarget.normalized);

                if (angle < m_LookAtMaxAngle)
                {
                    targetWeight = m_LookAtMaxWeight * (1f - angle / m_LookAtMaxAngle);

                    m_CurrentLookAtPos = target.position + Vector3.up * m_LookAtTargetHeight;
                }
            }
        }
                
        // smooth transition, avoid sudden change
        m_CurrentLookWeight = Mathf.Lerp(
            m_CurrentLookWeight, targetWeight, Time.deltaTime * m_LookAtSmoothSpeed);

        // apply ik weight
        // params: totalWeight, bodyWeight, headWeight, eyeWeight, clampWeight
        m_Character.model.animator.SetLookAtWeight(m_CurrentLookWeight, 0.3f, 1f, 0f, 0.5f);

        if (m_CurrentLookWeight > 0.01f)
            m_Character.model.animator.SetLookAtPosition(m_CurrentLookAtPos);
    }
}
