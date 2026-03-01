using System;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class CharacterSensor : MonoBehaviour 
{
    private const int SPEED_CACHE_NUM = 3;

    private Vector3[] m_VelocityCache;
    private Vector3 m_VelocitySum = Vector3.zero;
    private int m_CacheIndex = 0;
    private ICharacterBehavior m_CharacterBehavior;
    private CapsuleCollider m_CapsuleCollider;
    private Rigidbody m_Rigidbody;    

    private bool m_IsGrounded = false;
    private bool m_FirstEnter = true;

    /// <summary>
    /// Whether character is on walkable ground
    /// </summary>
    public bool isGrounded => m_IsGrounded;
    /// <summary>
    /// Character's average speed on ground
    /// </summary>
    public Vector3 averageVelocity => m_VelocitySum / SPEED_CACHE_NUM;

    #region State Methods
    private void Awake()
    {
        m_VelocitySum = Vector3.zero;
        m_VelocityCache = new Vector3[SPEED_CACHE_NUM];
        for (int i = 0; i < SPEED_CACHE_NUM; ++i)
            m_VelocityCache[i] = Vector3.zero;

        m_CacheIndex = 0;
        m_FirstEnter = true;                
    }

    private void Start()
    {
        m_CapsuleCollider = GetComponent<CapsuleCollider>();
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if(CheckTouchGround())
            CacheVelocity();
    }    
    #endregion

    #region Main Methods
    public void Init(ICharacterBehavior behavior)
    {
        m_CharacterBehavior = behavior;
    }

    private bool CheckTouchGround()
    {
        bool touchGround = SphereCheckGround(GameConsts.Layer.Walkable, out RaycastHit hit);
        if (m_IsGrounded != touchGround || m_FirstEnter)
        {
            m_FirstEnter = false;
            m_IsGrounded = touchGround;
            if (m_IsGrounded)
                m_CharacterBehavior?.OnContactGround(hit.collider);
            else
                m_CharacterBehavior?.OnExitGround();
        }
        return touchGround;
    }

    private void CacheVelocity()
    {
        m_VelocitySum -= m_VelocityCache[m_CacheIndex];
        m_VelocityCache[m_CacheIndex] = m_Rigidbody.linearVelocity;
        m_VelocitySum += m_VelocityCache[m_CacheIndex];
        m_CacheIndex = (m_CacheIndex + 1) % SPEED_CACHE_NUM;
    }

    /// <summary>
    /// Check whether the character touches the ground
    /// </summary>
    /// <param name="characterTransform"></param>
    /// <param name="radius"></param>
    /// <param name="layerMask"></param>
    /// <param name="raycastHit"></param>
    /// <param name="skinWidth"></param>
    /// <param name="groundCheckOffset"></param>
    /// <returns></returns>
    public bool SphereCheckGround(Transform characterTransform, float radius, LayerMask layerMask, out RaycastHit hit, float skinWidth = 0f, float groundCheckOffset = 0f)
    {
        if (Physics.SphereCast(characterTransform.position + Vector3.up * groundCheckOffset, radius, Vector3.down, out hit,
            Mathf.Abs(groundCheckOffset - radius) + 2f * skinWidth, layerMask))
        {
            float angle = Vector3.Angle(characterTransform.up, hit.normal);
            return angle < 45f;
        }
        return false;
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
        return SphereCheckGround(this.transform, m_CapsuleCollider.radius, layerMask, out raycastHit, skinWidth, groundCheckOffset);
    }
    #endregion
}
