using UnityEngine;

/// <summary>
/// Handles all ground locomotion: Walk/Run, StrafeMove, and Sprint.
/// Replaces the state-machine-driven HandleInput transitions with Ability-system-driven logic.
/// 
/// grantedTags should contain: Root.Locomotion (auto added/removed by base class)
/// Sub-mode tags (Move / Strafing / Sprint) are manually managed as the mode changes.
/// </summary>
public class LocomotionAbility : GameplayAbility
{
    /// <summary>
    /// Current sub-mode of locomotion.
    /// </summary>
    private enum LocomotionMode
    {
        None,
        Move,
        StrafeMove,
        Sprint
    }

    private LocomotionMode m_CurrentMode = LocomotionMode.None;
    private PlayerController m_Player;
    private PlayerActionController m_Action;

    // Cached tag instances for performance.
    private GameplayTag m_TagMove;
    private GameplayTag m_TagStrafing;
    private GameplayTag m_TagSprint;
    private GameplayTag m_TagHardLock;

    #region Ability Lifecycle

    protected override void OnAbilityActivated()
    {
        m_Player = m_Character as PlayerController;
        m_Action = m_Player != null ? m_Player.action : null;
        m_CurrentMode = LocomotionMode.None;

        // Cache tag instances.
        m_TagMove = GameplayTag.CreateTag(GameplayTagManager.instance.GetIndex(GameplayTag.LOCOMOTION_MOVE));
        m_TagStrafing = GameplayTag.CreateTag(GameplayTagManager.instance.GetIndex(GameplayTag.LOCOMOTION_STRAFING));
        m_TagSprint = GameplayTag.CreateTag(GameplayTagManager.instance.GetIndex(GameplayTag.LOCOMOTION_SPRINT));
        m_TagHardLock = GameplayTag.CreateTag(GameplayTagManager.instance.GetIndex(GameplayTag.COMBAT_LOCKED_HARD));
    }

    protected override void OnAbilityPerformed()
    {
        if (m_Player == null || m_Action == null)
        {
            EndAbility();
            return;
        }

        // Determine initial mode based on current input / lock state.
        LocomotionMode targetMode = ResolveTargetMode();
        ApplyMode(targetMode);
    }

    protected override void OnAbilityUpdate(float deltaTime)
    {
        if (m_Player == null || m_Action == null)
        {
            EndAbility();
            return;
        }

        // 1. If the state machine is no longer in a locomotion state (e.g. jumped/dodged/attacked),
        //    this ability should end so tags are cleaned up.
        if (!IsInLocomotionState())
        {
            EndAbility();
            return;
        }

        // 2. If the player stopped moving, end locomotion entirely.
        if (!m_Action.isMoving)
        {
            EndAbility();
            return;
        }

        // 3. Resolve which locomotion mode we should be in.
        LocomotionMode targetMode = ResolveTargetMode();

        // 4. Apply the mode if it changed.
        if (targetMode != m_CurrentMode)
        {
            ApplyMode(targetMode);
        }
    }

    protected override void OnAbilityEnded()
    {
        // Only return to Idle if we are still in a locomotion state.
        // (e.g. if we were already pushed into Attack by another ability, don't overwrite it.)
        if (m_Player != null && IsInLocomotionState())
        {
            m_Player.ChangeState(ECharacterState.Idle);
        }

        RemoveAllModeTags();
        m_CurrentMode = LocomotionMode.None;
    }

    protected override void OnAbilityCanceled()
    {
        if (m_Player != null && IsInLocomotionState())
        {
            m_Player.ChangeState(ECharacterState.Idle);
        }

        RemoveAllModeTags();
        m_CurrentMode = LocomotionMode.None;
    }

    protected override void OnAbilityReEnter()
    {
        if (m_Player == null || m_Action == null) return;

        // Re-evaluate the target mode.
        m_CurrentMode = LocomotionMode.None;
        LocomotionMode targetMode = ResolveTargetMode();
        ApplyMode(targetMode);
    }

    #endregion

    #region State Validation

    /// <summary>
    /// Returns true if the character's current state machine state is any locomotion variant.
    /// </summary>
    private bool IsInLocomotionState()
    {
        if (m_Player == null)
            return false;

        var currentState = m_Player.currentState;
        return currentState is PlayerStateMove
            || currentState is PlayerStateStrafeMove
            || currentState is PlayerStateSprint
            || currentState is PlayerStateIdle;
    }

    #endregion

    #region Mode Resolution

    /// <summary>
    /// Determines which locomotion sub-mode the player should be in based on current input and state.
    /// Priority: StrafeMove (Tag.Locked active) > Sprint > Move.
    ///
    /// Design note:
    ///   - Tag.Locked is granted by LockTargetAbility. If LockTargetAbility is blocked
    ///     (e.g. by Locomotion.Sprint via blockedTags), Tag.Locked won't be present,
    ///     and StrafeMove will NOT be entered — even if lockTarget is non-null.
    ///   - LockTargetAbility manages the lock-target lifecycle (WithinView check,
    ///     target null check) and will EndAbility when the lock should be released,
    ///     which removes Tag.Locked and triggers a switch back to Move/Sprint.
    ///   - lockTarget still serves as a secondary guard: even if Tag.Locked is stale
    ///     (e.g. one-frame delay), a null lockTarget prevents entering StrafeMove.
    /// </summary>
    private LocomotionMode ResolveTargetMode()
    {
        if (m_Player == null || m_Action == null)
            return LocomotionMode.None;

        // StrafeMove only when LockTargetAbility has set Tag.Locked and a target exists.
        // This respects blockedTags (e.g. Sprint blocks LockTargetAbility → Tag.Locked absent).
        bool isLockedOn = m_Player.lockTarget != null
            && m_ASC != null
            && m_ASC.HasTag(m_TagHardLock);

        if (isLockedOn)
        {
            return LocomotionMode.StrafeMove;
        }

        // Sprint when shouldSprint is true (Dodge key held past threshold).
        if (m_Action.shouldSprint)
        {
            return LocomotionMode.Sprint;
        }

        return LocomotionMode.Move;
    }

    /// <summary>
    /// Switches the state machine to the target locomotion mode and updates Ability tags.
    /// </summary>
    private void ApplyMode(LocomotionMode targetMode)
    {
        if (targetMode == m_CurrentMode || m_Player == null)
            return;

        // Remove tag for the previous mode (if any).
        RemoveModeTag(m_CurrentMode);

        // Switch state machine.
        ECharacterState state;
        switch (targetMode)
        {
            case LocomotionMode.StrafeMove:
                state = ECharacterState.StrafeMove;
                break;
            case LocomotionMode.Sprint:
                state = ECharacterState.Sprint;
                break;
            case LocomotionMode.Move:
            default:
                state = ECharacterState.Move;
                break;
        }

        m_Player.ChangeState(state);
        AddModeTag(targetMode);
        m_CurrentMode = targetMode;
    }

    #endregion

    #region Tag Helpers

    private void AddModeTag(LocomotionMode mode)
    {
        if (m_ASC == null) return;

        var tag = GetTagForMode(mode);
        if (tag.isValid)
            m_ASC.AddTag(tag);
    }

    private void RemoveModeTag(LocomotionMode mode)
    {
        if (m_ASC == null) return;

        var tag = GetTagForMode(mode);
        if (tag.isValid)
            m_ASC.RemoveTag(tag);
    }

    private void RemoveAllModeTags()
    {
        RemoveModeTag(LocomotionMode.Move);
        RemoveModeTag(LocomotionMode.StrafeMove);
        RemoveModeTag(LocomotionMode.Sprint);
    }

    private GameplayTag GetTagForMode(LocomotionMode mode)
    {
        switch (mode)
        {
            case LocomotionMode.Move:
                return m_TagMove;
            case LocomotionMode.StrafeMove:
                return m_TagStrafing;
            case LocomotionMode.Sprint:
                return m_TagSprint;
            default:
                return GameplayTag.RootTag; // invalid tag
        }
    }

    #endregion
}