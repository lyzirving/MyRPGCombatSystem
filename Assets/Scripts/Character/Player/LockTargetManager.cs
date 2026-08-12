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
    public string targetTag;

    public float eyeHeightOffset = 1.35f;

    public Color gizmosColor = Color.blue;  

    [Header("Hard Lock Config")]
    [SerializeField] private float m_HardLockConeHalfAngle = 30f;
    [SerializeField] private float m_MaxLockDistance = 30f;

    [Header("Soft Lock Config")]
    [SerializeField] private float m_SoftLockArcHalfAngle = 60f;
    [SerializeField] private float m_SoftLockMaxDistance = 15f;
    [SerializeField] private float m_SoftLockScanInterval = 0.2f;

    private CharacterControllerBase m_Character;
    
    private AbilitySystemComponent m_ASC;

    // Soft-lock state
    private Transform m_SoftLockTarget;
    private float m_LastSoftLockScanTime;
    private float m_HardUnlockTime = -1f;
    private const float HARD_UNLOCK_SETTLE_DELAY = 0.3f;

    /// <summary>
    /// whether have a hard lock on target
    /// </summary>
    public bool IsLocked => m_Character != null && m_Character.lockTarget != null;

    /// <summary>
    /// The currently hard locked target transform (null if not locked).
    /// </summary>
    public Transform LockedTarget => IsLocked ? m_Character.lockTarget : null;

    /// <summary>
    /// The current soft-lock target (nearest visible enemy in wide forward arc).
    /// Null when hard-locked. Updated at m_SoftLockScanInterval.
    /// </summary>
    public Transform SoftLockTarget => IsLocked ? null : m_SoftLockTarget;

    #region Unity Lifecycle

    private void Awake()
    {
        m_Character = GetComponent<CharacterControllerBase>();
        m_ASC = GetComponent<AbilitySystemComponent>();
    }

    private void Update()
    {
        ValidateLock();
        RefreshSoftLock();
    }

    private void OnDrawGizmos()
    {
        float fieldOfView = IsLocked ? m_HardLockConeHalfAngle : m_SoftLockArcHalfAngle;
        float sightDistance = IsLocked ? m_MaxLockDistance : m_SoftLockMaxDistance;
        DrawViewRange(fieldOfView * 2f, sightDistance);
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
        if (m_Character == null || m_ASC == null)
            return;

        if (IsLocked)
            return;

        // Soft→Hard promotion: prefer the current soft-lock target if still valid
        if (m_SoftLockTarget != null && IsValidHardLockTarget(m_SoftLockTarget))
        {
            m_Character.lockTarget = m_SoftLockTarget;
            m_ASC.TryActivateAbility<LockTargetAbility>();
            m_ASC.GetActive<LockTargetAbility>()?.SwitchToHardLock();
            return;
        }

        var cameraForward = GetCameraForward();
        var bestTarget = FindBestTargetInCone(
            cameraForward, m_HardLockConeHalfAngle, m_MaxLockDistance);

        if (bestTarget != null)
        {
            m_Character.lockTarget = bestTarget;
            m_ASC.TryActivateAbility<LockTargetAbility>();
            m_ASC.GetActive<LockTargetAbility>()?.SwitchToHardLock();
        }
    }

    /// <summary>
    /// Switch to the next visible target in the direction indicated by right-stick input.
    /// Falls back to sequential cycling (next in sorted list) when direction is near zero.
    /// </summary>
    /// <param name="stickDirection">Right-stick input vector (screen-space)</param>
    public void SwitchTarget(Vector2 stickDirection)
    {
        if (m_Character == null || m_ASC == null)
            return;

        if (!IsLocked)
            return;

        var visibleTargets = FindVisibleTargets(m_HardLockConeHalfAngle, m_MaxLockDistance);
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
            m_ASC.GetActive<LockTargetAbility>()?.SwitchToHardLock();
        }
    }

    /// <summary>
    /// Release the current lock target.
    /// </summary>
    public void UnlockTarget()
    {
        if (m_Character == null || m_ASC == null)
            return;

        bool wasLocked = m_Character.lockTarget != null;
        m_Character.lockTarget = null;
        m_ASC.CancelAbility<LockTargetAbility>();

        // Mark time for hard→soft settle delay
        if (wasLocked)
        {
            m_HardUnlockTime = Time.time;
        }
    }

    /// <summary>
    /// Returns all visible AI targets sorted by distance (nearest first).
    /// </summary>
    public List<Transform> FindVisibleTargets(float halfAngleDeg, float maxDistance)
    {
        var results = new List<Transform>();

        using (var iter = AIManager.instance.enumerator)
        {
            while (iter.MoveNext())
            {
                var character = iter.Current.Value;

                if (character == null) continue;

                if (CanSeeObject(character.transform, halfAngleDeg, maxDistance))
                    results.Add(character.transform);
            }
        }

        // Sort by distance (nearest first)
        results.Sort((a, b) =>
        {
            float distA = Vector3.Distance(transform.position, a.position);
            float distB = Vector3.Distance(transform.position, b.position);
            return distA.CompareTo(distB);
        });

        return results;
    }

    /// <summary>
    /// Finds the best target within a cone in front of the host.
    /// </summary>
    /// <param name="forward">Cone direction (typically camera forward, y=0 normalized)</param>
    /// <param name="halfAngleDeg">Half-angle of the cone in degrees</param>
    /// <param name="maxDistance">Maximum distance to consider</param>
    /// <returns>The best target or null</returns>
    public Transform FindBestTargetInCone(Vector3 forward, float halfAngleDeg, float maxDistance)
    {
        Transform bestTarget = null;
        float bestScore = float.MaxValue;

        using (var iter = AIManager.instance.enumerator)
        {
            while (iter.MoveNext())
            {
                var character = iter.Current.Value;
                if (character == null) continue;

                if (!CanSeeObject(character.transform, halfAngleDeg, maxDistance)) continue;

                // Score: prefer closer targets, slightly prefer targets closer to center                
                Vector3 toTarget = character.transform.position - transform.position;
                toTarget.y = 0;
                float distance = toTarget.magnitude;

                float angle = Vector3.Angle(forward, toTarget.normalized);

                float score = distance + angle * 0.1f;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = character.transform;
                }
            }
        }
        return bestTarget;
    }

    public bool CanSeeObject(Transform target, float halfAngleDeg, float maxDistance)
    { 
        if (target == null) return false;

        if (!string.IsNullOrEmpty(targetTag) && !target.gameObject.CompareTag(targetTag)) return false;

        Vector3 eyePosition = transform.position + transform.up * eyeHeightOffset;
        float dist = Vector3.Distance(eyePosition, target.position);

        if (dist > maxDistance) return false;

        Vector3 dir = target.position - transform.position;
        dir.Normalize();

        return IsDirectionInView(dir, halfAngleDeg);
    }

    public bool IsDirectionInView(Vector3 direction, float halfAngleDeg)
    {
        float dot = Vector3.Dot(direction, transform.forward);
        if (dot < 0f) return false;

        float angle = Vector3.Angle(transform.forward, direction);
        return angle < halfAngleDeg;
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
        if (!CanSeeObject(target, m_HardLockConeHalfAngle, m_MaxLockDistance))
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
    /// Interval-based soft-lock scan. Clears target when hard-locked, defers
    /// recalculation after hard unlock (settle delay), otherwise scans at
    /// m_SoftLockScanInterval using wide forward arc.
    /// </summary>
    private void RefreshSoftLock()
    {
        if (IsLocked)
        {
            m_SoftLockTarget = null;
            return;
        }

        // Hard→Soft settle: don't recalculate immediately after unlock
        if (m_HardUnlockTime > 0f && Time.time - m_HardUnlockTime < HARD_UNLOCK_SETTLE_DELAY)
            return;

        m_HardUnlockTime = -1f;

        // Interval-based scanning (not every frame)
        if (Time.time - m_LastSoftLockScanTime < m_SoftLockScanInterval)
            return;

        m_LastSoftLockScanTime = Time.time;

        if (m_Character == null)
        {
            m_SoftLockTarget = null;
            return;
        }

        var preSoftLockTarget = m_SoftLockTarget;
        m_SoftLockTarget = FindBestTargetInCone(
            GetCameraForward(), m_SoftLockArcHalfAngle, m_SoftLockMaxDistance);

        if(m_SoftLockTarget == preSoftLockTarget)
            return;

        // Sync LockTargetAbility with soft-lock state
        if (m_SoftLockTarget != null)
        {
            // Found a target: ensure ability is active and in soft-lock mode
            if (m_ASC.GetActive<LockTargetAbility>() == null)
                m_ASC.TryActivateAbility<LockTargetAbility>();

            m_ASC.GetActive<LockTargetAbility>()?.SwitchToSoftLock();
        }
        else
        {
            // No target visible: release the ability
            m_ASC.CancelAbility<LockTargetAbility>();
        }
    }

    /// <summary>
    /// Checks whether a target still meets hard-lock criteria
    /// (visible, within cone, within max distance). Used for soft→hard promotion.
    /// </summary>
    private bool IsValidHardLockTarget(Transform target)
    {
        if (target == null) return false;
        if (!CanSeeObject(target, m_HardLockConeHalfAngle, m_MaxLockDistance)) return false;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0;
        float dist = toTarget.magnitude;
        if (dist > m_MaxLockDistance || dist < 0.01f) return false;

        float angle = Vector3.Angle(GetCameraForward(), toTarget.normalized);
        return angle <= m_HardLockConeHalfAngle;
    }

    /// <summary>
    /// From a list of candidate targets, pick the one whose screen-space position
    /// best matches the right-stick input direction.
    /// </summary>
    private Transform PickBestInDirection(List<Transform> candidates, Vector2 stickDir)
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

    private void DrawViewRange(float fieldOfView, float sightDistance)
    {
        Vector3 eyePosition = transform.position + transform.up * eyeHeightOffset;
        Vector3 dir1 = Quaternion.AngleAxis(fieldOfView / 2, transform.up) * transform.forward;
        Vector3 dir2 = Quaternion.AngleAxis(-fieldOfView / 2, transform.up) * transform.forward;
        dir1.Normalize();
        dir2.Normalize();
        Vector3 startPt = eyePosition + dir1 * sightDistance;
        Vector3 endPt = eyePosition + dir2 * sightDistance;

        Debug.DrawLine(eyePosition, startPt, gizmosColor);
        Debug.DrawLine(eyePosition, endPt, gizmosColor);

        int itr = 20;
        float interval = fieldOfView / itr;
        float currentAngle = fieldOfView / 2;
        Vector3 lastPt = startPt;
        Vector3 currentAnglePt = startPt;
        for (int i = 1; i < itr + 1; ++i)
        {
            currentAngle -= interval;
            Vector3 d = Quaternion.AngleAxis(currentAngle, transform.up) * transform.forward;
            d.Normalize();
            currentAnglePt = eyePosition + d * sightDistance;
            Debug.DrawLine(lastPt, currentAnglePt, gizmosColor);
            lastPt = currentAnglePt;
        }
    }
    #endregion
}