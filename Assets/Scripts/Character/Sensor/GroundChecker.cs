using UnityEngine;

public class GroundChecker
{
    public delegate void TouchGroundNotify(Collider collider);
    public delegate void ExitGroundNotify();

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
        if (Physics.SphereCast(transform.position + Vector3.up * groundCheckOffset, radius, Vector3.down, out hit,
            Mathf.Abs(groundCheckOffset - radius) + 2f * skinWidth, layerMask))
        {
            float angle = Vector3.Angle(transform.up, hit.normal);
            return angle < 45f;
        }
        return false;
    }
}
