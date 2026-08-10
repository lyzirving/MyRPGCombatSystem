using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core scheduler for hard lock-on system.
/// 
/// Responsibilities:
///   - lockTarget management (single writer to CharacterControllerBase.lockTarget)
///   - Target selection: cone-based initial lock + cycling through visible targets
///   - Lock validation: distance, visibility, existence checks every frame
///   - ASC interaction: activate/cancel LockTargetAbility
/// 
/// Phase 1: Core hard-lock with on-demand sensor queries.
/// Phase 2 (future): Input bindings via PlayerActionController.
/// Phase 5 (future): Soft-lock integration.
/// </summary>
public class LockTargetManager : MonoBehaviour
{
    [Header("Hard Lock Config")]
    [SerializeField] private float m_HardLockConeHalfAngle = 30f;
    [SerializeField] private float m_MaxLockDistance = 30f;

    private CharacterControllerBase m_Character;
    private CharacterSensor m_Sensor;
    private AbilitySystemComponent m_ASC;

    public bool IsLocked => m_Character != null
        && m_Character.lockTarget != null
        && m_ASC != null
        && m_ASC.GetActive<LockTargetAbility>() != null;

    /// <summary>
    /// The currently locked target transform (null if not locked).
    /// Read by camera/orbit systems.
    /// </summary>
    public Transform LockedTarget => IsLocked ? m_Character.lockTarget : null;

    #region Unity Lifecycle

    private void Awake()
    {
        m_Character = GetComponent<CharacterControllerBase>();
        m_Sensor = GetComponent<CharacterSensor>();
        m_ASC = GetComponent<AbilitySystemComponent>();
    }

    private void Update()
    {
        ValidateLock();
    }

    #endregion

    #region Public API (called by PlayerActionController input handlers)

    /// <summary>
    /// Toggle lock-on: locks if not locked, unlocks if locked.
    /// </summary>
    public void ToggleLock()
    {
        if (IsLocked)
        {
            UnlockTarget();
        }
        else
        {
            TryLockTarget();
        }
    }

    /// <summary>
    /// Lock onto the best target in front of the camera.
    /// Does nothing if already locked (use SwitchTarget to cycle).
    /// </summary>
    public void TryLockTarget()
    {
        if (m_Character == null || m_Sensor == null || m_ASC == null)
            return;

        if (IsLocked)
            return;

        var cameraForward = GetCameraForward();
        var bestTarget = m_Sensor.FindBestTargetInCone(
            cameraForward, m_HardLockConeHalfAngle, m_MaxLockDistance);

        if (bestTarget != null)
        {
            m_Character.lockTarget = bestTarget;
            m_ASC.TryActivateAbility<LockTargetAbility>();
        }
    }

    /// <summary>
    /// Switch to the next visible target in the direction indicated by right-stick input.
    /// Falls back to sequential cycling (next in sorted list) when direction is near zero.
    /// </summary>
    /// <param name="stickDirection">Right-stick input vector (screen-space)</param>
    public void SwitchTarget(Vector2 stickDirection)
    {
        if (m_Character == null || m_Sensor == null || m_ASC == null)
            return;

        if (!IsLocked)
            return;

        var visibleTargets = m_Sensor.FindVisibleTargets();
        if (visibleTargets.Count <= 1)
        {
            // Single target or none — nothing to switch to.
            if (visibleTargets.Count == 0)
                UnlockTarget();
            return;
        }

        // Remove current target from candidates so we always switch away.
        visibleTargets.RemoveAll(t => t == m_Character.lockTarget);
        if (visibleTargets.Count == 0)
            return;

        Transform nextTarget;
        if (stickDirection.sqrMagnitude > 0.1f)
        {
            // Direction-aware switching: pick best match for the stick direction.
            nextTarget = PickBestInDirection(visibleTargets, stickDirection);
        }
        else
        {
            // Sequential fallback: nearest target.
            nextTarget = visibleTargets[0];
        }

        if (nextTarget != null)
        {
            m_Character.lockTarget = nextTarget;
            m_ASC.TryActivateAbility<LockTargetAbility>();
        }
    }

    /// <summary>
    /// Release the current lock target.
    /// </summary>
    public void UnlockTarget()
    {
        if (m_Character == null || m_ASC == null)
            return;

        m_Character.lockTarget = null;
        m_ASC.CancelAbility<LockTargetAbility>();
    }

    #endregion

    #region Validation

    /// <summary>
    /// Every frame, validate that the current lock target is still valid.
    /// Invalidates the lock if: target destroyed, too far, or not visible.
    /// </summary>
    private void ValidateLock()
    {
        if (!IsLocked)
            return;

        var target = m_Character.lockTarget;

        // 1. Target destroyed / externally cleared
        if (target == null)
        {
            UnlockTarget();
            return;
        }

        // 2. Too far
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > m_MaxLockDistance)
        {
            UnlockTarget();
            return;
        }

        // 3. No longer visible
        if (!m_Sensor.CanSeeObject(target))
        {
            UnlockTarget();
            return;
        }
    }

    #endregion

    #region Helpers

    private static Vector3 GetCameraForward()
    {
        if (Camera.main == null)
            return Vector3.forward;

        var forward = Camera.main.transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    /// <summary>
    /// From a list of candidate targets, pick the one whose screen-space position
    /// best matches the right-stick input direction.
    /// </summary>
    private Transform PickBestInDirection(System.Collections.Generic.List<Transform> candidates, Vector2 stickDir)
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        // Desired angle from the stick direction: 0° = camera forward, +90° = right, -90° = left
        float desiredAngle = Mathf.Atan2(stickDir.x, stickDir.y) * Mathf.Rad2Deg;

        Transform bestTarget = null;
        float bestDelta = float.MaxValue;

        foreach (var target in candidates)
        {
            float targetAngle = GetHorizontalAngleToTarget(target);
            float delta = Mathf.Abs(Mathf.DeltaAngle(targetAngle, desiredAngle));

            if (delta < bestDelta)
            {
                bestDelta = delta;
                bestTarget = target;
            }
        }

        return bestTarget;
    }

    /// <summary>
    /// Returns the horizontal signed angle (degrees) from the camera forward to the target.
    /// Positive = target is to the right of the camera, negative = to the left.
    /// </summary>
    private float GetHorizontalAngleToTarget(Transform target)
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        var camForward = GetCameraForward();
        var camRight = Vector3.Cross(Vector3.up, camForward).normalized;

        // Project target direction onto camera forward/right plane
        float forwardDot = Vector3.Dot(toTarget.normalized, camForward);
        float rightDot = Vector3.Dot(toTarget.normalized, camRight);

        return Mathf.Atan2(rightDot, forwardDot) * Mathf.Rad2Deg;
    }

    #endregion
}