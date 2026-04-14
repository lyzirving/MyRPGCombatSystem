using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CharacterControllerBase : MonoBehaviour, IStateMachineOwner, ICharacterBehavior
{
    [SerializeField] protected CharacterConfig m_Config = new CharacterConfig();
    protected CharacterAttrs m_Attrs = new CharacterAttrs();
    protected StateMachine m_StateMachine;

    protected int m_CharacterGUID = GUIDConsts.PlayerAnimation;
    protected Rigidbody m_Rigidbody;    
    protected CharacterSensor m_Sensor;
    protected CapsuleCollider m_CapsuleCollider;
    protected AttackComponent m_AttackComponent;
    protected DistanceZone m_DistanceZone;

    public CharacterConfig config => m_Config;
    public CharacterAttrs attrs => m_Attrs;
    public Rigidbody rigidBody => m_Rigidbody;
    public CharacterSensor sensor => m_Sensor;
    public CapsuleCollider capsule => m_CapsuleCollider;
    public AttackComponent attackComponent { get => m_AttackComponent; }

    public StateBase currentState => m_StateMachine.currentState;
    public float speedScaler => m_Config.baseSpeed * m_Attrs.speedModify;
    public float walkSpeedScaler => m_Config.baseSpeed * m_Config.walkSpeedModify;
    public float runSpeedScaler => m_Config.baseSpeed * m_Config.runSpeedModify;

    public Vector3 verticalVelocity => new Vector3(0f, m_Rigidbody.linearVelocity.y, 0f);
    public Vector3 horizontalVelocity => new Vector3(m_Rigidbody.linearVelocity.x, 0f, m_Rigidbody.linearVelocity.z);
    public bool isMovingUp => m_Rigidbody.linearVelocity.y > 0f;
    public bool isMoveHorizontally => horizontalVelocity.magnitude > Mathf.Epsilon;
    public Vector3 cameraDirection => new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z).normalized;
    public Quaternion cameraRotation => Quaternion.Euler(new Vector3(0f, Camera.main.transform.eulerAngles.y, 0f));

    #region Virtual Methods
    public virtual bool IsInAnimationTransition(int layer = 0) { return false; }
    #endregion

    #region Main Methods
    public ECharacterAction GetCurrentAction() { return m_StateMachine.currentState.GetCurrentAction(); }

    /// <summary>
    /// v += f * dt
    /// </summary>
    /// <param name="force"></param>
    public void Move(in Vector3 force)
    {
        MoveByForceMode(force, ForceMode.Acceleration);
    }

    /// <summary>
    /// v += f
    /// </summary>
    /// <param name="force"></param>
    public void MoveImmediately(in Vector3 force)
    {
        MoveByForceMode(force, ForceMode.VelocityChange);
    }

    public void MoveToImmediately(Transform target, float speed = 1f, float rotationSpeed = 1f)
    {
        if (target == null) return;
        if (target.position == transform.position) return;

        var targetPos = target.position;        
        targetPos.y = transform.position.y;
        Vector3 targetDir = targetPos - transform.position;
        targetDir.Normalize();

        RotateToTargetDir(targetDir, rotationSpeed);
        MoveImmediately(targetDir * speed - horizontalVelocity);
    }

    public void MoveByForceMode(in Vector3 force, ForceMode forceMode)
    {
        m_Rigidbody.AddForce(force, forceMode);
    }    

    public void RotateToTargetDir(Vector3 targetDir, float rotateSpeed = 1f)
    {
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
            Quaternion.LookRotation(targetDir), Time.deltaTime * rotateSpeed);
    }

    public void ResetVelocity()
    {
        m_Rigidbody.linearVelocity = Vector3.zero;
    }

    public void ResetHorizontalVelocity()
    {
        var y = m_Rigidbody.linearVelocity.y;
        m_Rigidbody.linearVelocity = new Vector3(0f, y, 0f);
    }

    public void ResetVerticalVelocity()
    {
        m_Rigidbody.linearVelocity = horizontalVelocity;
    }

    protected void Init()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_CapsuleCollider = GetComponent<CapsuleCollider>();
        m_Sensor = this.transform.AddComponent<CharacterSensor>();
        m_Sensor.Init(this);

        m_StateMachine = new StateMachine();
        m_StateMachine.Init(this);

        m_AttackComponent = GetComponent<AttackComponent>();
        m_AttackComponent?.Init(this);

        m_DistanceZone = GetComponent<DistanceZone>();
    }
    #endregion

    #region IStateMachineOwner Methods
    public virtual void ChangeState(ECharacterState state, ChangeStateArgs args = default(ChangeStateArgs))
    {        
    }

    public void ExitCurrentState()
    {
        m_StateMachine?.ExitCurrentState();
    }

    public virtual void AddAdditiveState(ECharacterState state, ChangeStateArgs args = default(ChangeStateArgs))
    { 
    }

    public virtual void RemoveAdditiveState(ECharacterState state)
    {
    }
    #endregion

    #region ICharacterBehavior Methods
    public virtual bool isLightAttack => false;

    public virtual Transform modelTransform => null;
    public int GUID => m_CharacterGUID;

    public virtual void OnAttackBegin() 
    {
        m_AttackComponent?.attackBox.OnAttackBegin();
    }

    public virtual void OnAttackEnd() 
    {
        m_AttackComponent?.attackBox.OnAttackEnd();
    }

    public virtual void OnAttackHit(ICharacterBehavior target, Vector3 hitPos) { }

    public virtual void OnHit(Vector3 hitPos, in ICharacterBehavior source, in SkillData skillData) { }

    public virtual void OnFootStep(EFootstep footStep) { }

    public virtual void OnContactGround(Collider collider) 
    {
        var state = m_StateMachine.currentState as CharacterStateBase;
        state?.OnContactGround(collider);
    }

    public virtual void OnExitGround() 
    {
        var state = m_StateMachine.currentState as CharacterStateBase;
        state?.OnExitGround();
    }    
    #endregion
}
