using UnityEngine;
using System;

/// <summary>
/// Ground detection driven by a downward sphere cast instead of OnCollisionStay contacts.
/// Every physics frame we probe below the capsule for the nearest non-self surface, then:
///   1. only surfaces within GROUND_SLOPE_LIMIT count as ground (slope angle),
///   2. the character is snapped down onto the surface (贴地吸附),
///   3. any residual downward velocity is cancelled so gravity doesn't accumulate.
/// This is deterministic (it does not rely on transient physics contact callbacks) and gives
/// a small spatial tolerance, so fast movement over slopes no longer reads as "left ground".
/// </summary>
[Serializable]
public class GroundChecker
{
    public delegate void TouchGroundNotify(Collider collider);
    public delegate void ExitGroundNotify();

    [Tooltip("Maximum walkable slope angle, in degrees.")]
    public float GROUND_SLOPE_LIMIT = 45f;

    [Tooltip("How far below the feet to probe for ground.")]
    public float GROUND_PROBE_DISTANCE = 0.25f;

    [Tooltip("Maximum distance to snap the character down to the ground.")]
    public float GROUND_SNAP_DISTANCE = 0.25f;

    [Tooltip("Draw the ground probe (start/end spheres + hit normal) in the Scene view.")]
    public bool debugDrawProbe = false;

    private bool m_IsGrounded = false;
    private Collider m_GroundCollider = null;
    private Vector3 m_GroundNormal = Vector3.up;

    private Rigidbody m_Rigidbody;
    private CapsuleCollider m_Capsule;
    private int m_GroundLayerMask;

    private readonly RaycastHit[] m_HitBuffer = new RaycastHit[8];

    // Last probe result, used by CharacterSensor to draw debug gizmos.
    private bool m_LastHitValid = false;
    private Vector3 m_LastHitPoint = Vector3.zero;
    private Vector3 m_LastHitNormal = Vector3.up;

    private TouchGroundNotify m_TouchGroundNotify;
    private ExitGroundNotify m_ExitGroundNotify;

    public TouchGroundNotify onTouch
    {
        get => m_TouchGroundNotify;
        set => m_TouchGroundNotify = value;
    }

    public ExitGroundNotify onExit
    {
        get => m_ExitGroundNotify;
        set => m_ExitGroundNotify = value;
    }

    public bool isGrounded => m_IsGrounded;

    /// <summary>World-space normal of the surface the character is currently standing on.</summary>
    public Vector3 groundNormal => m_GroundNormal;

    /// <summary>Whether the last probe found any non-self surface (for gizmos).</summary>
    public bool debugHasHit => m_LastHitValid;
    public Vector3 debugHitPoint => m_LastHitPoint;
    public Vector3 debugHitNormal => m_LastHitNormal;

    public void Init(Rigidbody rigidbody, CapsuleCollider capsule, int groundLayerMask)
    {
        m_Rigidbody = rigidbody;
        m_Capsule = capsule;
        m_GroundLayerMask = groundLayerMask;
    }

    /// <summary>
    /// Call once per physics frame (in FixedUpdate, before the next physics step).
    /// Probes for ground, snaps the character onto it and fires onTouch/onExit.
    /// </summary>
    public void Tick()
    {
        if (m_Capsule == null || m_Rigidbody == null)
        {
            SetGrounded(false, null, Vector3.up);
            return;
        }

        if (ProbeGround(out RaycastHit hit))
        {
            SnapToGround(hit);
            CancelDownwardVelocity();
            SetGrounded(true, hit.collider, hit.normal);
        }
        else
        {
            SetGrounded(false, null, Vector3.up);
        }
    }

    private bool ProbeGround(out RaycastHit outHit)
    {
        outHit = default;
        m_LastHitValid = false;

        // Cast from the capsule centre so the sphere never starts inside the ground
        // (starting inside a collider would make the cast miss it).
        Vector3 center = m_Capsule.bounds.center;
        float halfHeight = m_Capsule.height * 0.5f - m_Capsule.radius;
        float maxDistance = halfHeight + GROUND_PROBE_DISTANCE + 0.01f;

        int count = Physics.SphereCastNonAlloc(
            center,
            m_Capsule.radius,
            Vector3.down,
            m_HitBuffer,
            maxDistance,
            m_GroundLayerMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = m_HitBuffer[i];

            // Ignore any collider attached to the character's own rigidbody.
            if (hit.rigidbody == m_Rigidbody)
                continue;

            // Record the first non-self surface for gizmo visualization.
            if (!m_LastHitValid)
            {
                m_LastHitValid = true;
                m_LastHitPoint = hit.point;
                m_LastHitNormal = hit.normal;
            }

            // Steeper than the slope limit -> treat as a wall, not ground.
            if (Vector3.Angle(Vector3.up, hit.normal) > GROUND_SLOPE_LIMIT)
                continue;

            // hit.distance is measured from the cast origin (capsule centre); the capsule's
            // lowest point is (halfHeight + radius) below the centre, so the gap between the
            // feet and the ground is hit.distance - halfHeight.
            float gap = hit.distance - halfHeight;
            if (gap <= GROUND_PROBE_DISTANCE)
            {
                outHit = hit;
                return true;
            }

            // The closest walkable surface is already too far away.
            return false;
        }

        return false;
    }

    private void SnapToGround(RaycastHit hit)
    {
        // Never snap while moving up (an active jump), or the jump gets cancelled.
        if (m_Rigidbody.linearVelocity.y > 0f)
            return;

        float halfHeight = m_Capsule.height * 0.5f - m_Capsule.radius;

        // gap > 0 means the feet are floating this far above the ground.
        float gap = hit.distance - halfHeight;

        if (gap > 0f && gap <= GROUND_SNAP_DISTANCE)
        {
            Vector3 position = m_Rigidbody.position;
            position.y -= gap;
            m_Rigidbody.MovePosition(position);
        }
    }

    private void CancelDownwardVelocity()
    {
        Vector3 velocity = m_Rigidbody.linearVelocity;
        if (velocity.y < 0f)
        {
            velocity.y = 0f;
            m_Rigidbody.linearVelocity = velocity;
        }
    }

    private void SetGrounded(bool grounded, Collider collider, Vector3 normal)
    {
        bool changed = m_IsGrounded != grounded;
        m_IsGrounded = grounded;
        
        m_GroundCollider = grounded ? collider : null;
        m_GroundNormal = grounded ? normal : Vector3.up;

        if (changed)
        {
            if (m_IsGrounded)
                m_TouchGroundNotify?.Invoke(m_GroundCollider);
            else
                m_ExitGroundNotify?.Invoke();
        }
    }
}
