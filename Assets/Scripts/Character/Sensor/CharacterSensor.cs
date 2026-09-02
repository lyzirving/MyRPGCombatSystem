using UnityEngine;

public class CharacterSensor : MonoBehaviour 
{
    [SerializeField] private DistanceZone m_DistanceZone = new DistanceZone();
    [SerializeField] private GroundChecker m_GroundChecker;
    private VelocityCache m_VelocityCache;
    private ICharacterBehavior m_CharacterBehavior;    

    /// <summary>
    /// Whether character is standing on a collidable surface (any collider, layer-agnostic)
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

        m_GroundChecker = new GroundChecker();
        m_GroundChecker.onTouch += m_CharacterBehavior.OnContactGround;
        m_GroundChecker.onExit += m_CharacterBehavior.OnExitGround;

        m_DistanceZone.host = transform;
    }
    #endregion

    #region State Methods
    private void Update()
    {
        m_DistanceZone.UpdateDistance();
    }

    private void FixedUpdate()
    {
        // Resolve grounded state from the previous physics step's collision contacts.
        m_GroundChecker.Tick();

        if (m_GroundChecker.isGrounded)
            m_VelocityCache.UpdateVelocity();
    }

    private void OnCollisionStay(Collision collision)
    {
        m_GroundChecker.OnCollisionStay(collision);
    }
    #endregion    
}
