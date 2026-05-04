using UnityEngine;

public class CharacterSensor : MonoBehaviour 
{
    [SerializeField] private ViewScanner m_ViewScanner = new ViewScanner();
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

        m_ViewScanner.Init(this.transform);
        m_ViewScanner.onFind += m_CharacterBehavior.OnTargetFind;
        m_ViewScanner.onLost += m_CharacterBehavior.OnTargetLost;
        m_ViewScanner.onChange += m_CharacterBehavior.OnTargetChange;

        m_DistanceZone.host = this.transform;
    }
    #endregion

    #region Sensor Methods
    public bool WithinView(Vector3 direction)
    {
        return m_ViewScanner.WithinView(direction);
    }

    public bool CanSeeObject(Transform transform)
    {
        return m_ViewScanner.CanSeeObject(transform);
    }
    #endregion

    #region State Methods
    private void Update()
    {
        m_ViewScanner.Scan();
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
        m_ViewScanner?.DrawGizmos();
    }
    #endregion    
}
