using UnityEngine;

public class GroundChecker
{
    public delegate void TouchGroundNotify(Collider collider);
    public delegate void ExitGroundNotify();

    /// <summary>
    /// Probe offsets (normalized) for the ground check, scaled by radius*0.6 around the
    /// capsule bottom. Center + 4 cardinal points cover edge landings without a sphere sweep.
    /// </summary>
    private static readonly Vector3[] GROUND_PROBE_DIRS =
    {
        Vector3.zero,
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right
    };

    private CapsuleCollider m_CapsuleCollider;
    private Transform m_Transform;

    private bool m_IsGrounded = false;
    private bool m_FirstEnter = true;

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

    public GroundChecker(Transform transform, CapsuleCollider collider)
    { 
        m_Transform = transform;
        m_CapsuleCollider = collider;
        m_FirstEnter = true;
        m_IsGrounded = false;
    }

    public bool CheckTouchGround(LayerMask layerMask)
    {
        bool touchGround = SphereCheckGround(layerMask, out RaycastHit hit);
        if (m_IsGrounded != touchGround || m_FirstEnter)
        {
            m_FirstEnter = false;
            m_IsGrounded = touchGround;

            if (m_IsGrounded)
                m_TouchGroundNotify?.Invoke(hit.collider);
            else
                m_ExitGroundNotify?.Invoke();
        }
        return touchGround;
    }

    /// <summary>
    /// SphereCheckGround using Character's attribute
    /// </summary>
    /// <param name="layerMask"></param>
    /// <param name="raycastHit"></param>
    /// <param name="skinWidth"></param>
    /// <param name="groundCheckOffset"></param>
    /// <returns></returns>
    public bool SphereCheckGround(LayerMask layerMask, out RaycastHit raycastHit, float skinWidth = 0.1f, float groundCheckOffset = 0.5f)
    {
        return SphereCheckGround(m_Transform, m_CapsuleCollider.radius, layerMask, out raycastHit, skinWidth, groundCheckOffset);
    }

    /// <summary>
    /// Check whether the character touches the ground
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="radius"></param>
    /// <param name="layerMask"></param>
    /// <param name="hit"></param>
    /// <param name="skinWidth"></param>
    /// <param name="groundCheckOffset"></param>
    /// <returns></returns>
    public bool SphereCheckGround(Transform transform, float radius, LayerMask layerMask, out RaycastHit hit, float skinWidth = 0f, float groundCheckOffset = 0f)
    {
        const float groundSlopeLimit = 45f;
        float checkDistance = Mathf.Abs(groundCheckOffset - radius) + 2f * skinWidth;
        float probeRadius = radius * 0.6f;

        for (int i = 0; i < GROUND_PROBE_DIRS.Length; i++)
        {
            // Thin rays cast straight down from the capsule bottom. They only detect what is
            // directly below the capsule, so a side obstacle's top face can never be mistaken
            // for the ground while the character is pressed against it (a sphere sweep would
            // hit that top face and wrongly report "grounded").
            Vector3 origin = transform.position + GROUND_PROBE_DIRS[i] * probeRadius + Vector3.up * groundCheckOffset;
            if (Physics.Raycast(origin, Vector3.down, out hit, checkDistance, layerMask))
            {
                if (Vector3.Angle(transform.up, hit.normal) < groundSlopeLimit)
                    return true;
            }
        }

        hit = default;
        return false;
    }
}
