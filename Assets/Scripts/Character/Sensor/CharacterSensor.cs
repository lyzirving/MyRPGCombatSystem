using UnityEngine;
using System.Collections.Generic;

public class CharacterSensor : MonoBehaviour 
{
    [SerializeField] private ViewChecker m_ViewChecker = new ViewChecker();
    [SerializeField] private DistanceZone m_DistanceZone = new DistanceZone();
    private GroundChecker m_GroundChecker;
    private VelocityCache m_VelocityCache;
    private ICharacterBehavior m_CharacterBehavior;    

    /// <summary>
    /// Whether character is on walkable ground
    /// </summary>
    public bool isGrounded => m_GroundChecker.isGrounded;
    /// <summary>
    /// Character's average speed on ground
    /// </summary>
    public Vector3 averageVelocity => m_VelocityCache.averageVelocity;
    public DistanceZone distZone => m_DistanceZone;

    #region Main Methods
    public void Init(ICharacterBehavior behavior)
    {
        m_CharacterBehavior = behavior;

        m_VelocityCache = new VelocityCache(GetComponent<Rigidbody>());

        m_GroundChecker = new GroundChecker(this.transform, GetComponent<CapsuleCollider>());
        m_GroundChecker.onTouch += m_CharacterBehavior.OnContactGround;
        m_GroundChecker.onExit += m_CharacterBehavior.OnExitGround;

        m_ViewChecker.host = this.transform;

        m_DistanceZone.host = this.transform;
    }
    #endregion

    #region Sensor Methods
    public bool WithinView(Vector3 direction)
    {
        return m_ViewChecker.IsDirectionInView(direction);
    }

    public bool CanSeeObject(Transform transform)
    {
        return m_ViewChecker.CanSeeObject(transform);
    }

    /// <summary>
    /// Returns all visible AI targets sorted by distance (nearest first).
    /// Used by LockTargetManager for target switching.
    /// </summary>
    public List<Transform> FindVisibleTargets()
    {
        return m_ViewChecker.FindVisibleTargets();
    }

    /// <summary>
    /// Finds the best target within a cone in front of this character.
    /// Used by LockTargetManager for initial hard-lock acquisition.
    /// </summary>
    public Transform FindBestTargetInCone(Vector3 forward, float halfAngleDeg, float maxDistance)
    {
        return m_ViewChecker.FindBestTargetInCone(forward, halfAngleDeg, maxDistance);
    }
    #endregion

    #region State Methods
    private void Update()
    {
        m_DistanceZone.UpdateDistance();
        m_GroundChecker.CheckTouchGround(GameConsts.Layer.Walkable);
    }

    private void FixedUpdate()
    {
        if (m_GroundChecker.isGrounded)
            m_VelocityCache.UpdateVelocity();
    }

    private void OnDrawGizmos()
    {
        m_ViewChecker?.DrawViewRange();
    }
    #endregion    
}
