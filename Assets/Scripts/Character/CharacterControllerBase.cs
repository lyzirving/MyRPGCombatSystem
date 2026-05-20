using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(CharacterSensor))]
[RequireComponent(typeof(AudioPool))]
public class CharacterControllerBase : MonoBehaviour, IStateMachineOwner, ICharacterBehavior
{
    [SerializeField] protected CharacterConfig m_Config = new CharacterConfig();
    [SerializeField] protected GameplayAbilitySet m_AbilitySet = null;

    protected CharacterAttrs m_Attrs = new CharacterAttrs();
    protected StateMachine m_StateMachine;
    protected CharacterModel m_Model;

    protected int m_CharacterGUID = GUIDConsts.PlayerAnimation;
    protected ECharacterDodgeAction m_DodgeAction = ECharacterDodgeAction.None;
    protected CharacterSensor m_Sensor;
    protected AttackComponent m_AttackComponent;
    protected Rigidbody m_Rigidbody;
    protected CapsuleCollider m_CapsuleCollider;    
    protected AudioPool m_AudioPool;

    public CharacterConfig config => m_Config;
    public CharacterAttrs attrs => m_Attrs;
    public CharacterModel model => m_Model;
    public Rigidbody rigidBody => m_Rigidbody;
    public CharacterSensor sensor => m_Sensor;
    public CapsuleCollider capsule => m_CapsuleCollider;
    public AttackComponent attackComponent { get => m_AttackComponent; }
    public Transform lockTarget
    {
        get => m_Sensor.distZone.target;
        set => m_Sensor.distZone.target = value;
    }
    public StateBase currentState => m_StateMachine.currentState;    
    public float speedScaler => m_Config.move.baseSpeed * m_Attrs.speedModify;
    public float walkSpeedScaler => m_Config.move.baseSpeed * m_Config.move.walkModify;
    public float runSpeedScaler => m_Config.move.baseSpeed * m_Config.move.runModify;
    public ECharacterDodgeAction dodgeAction
    {
        get => m_DodgeAction;
        set => m_DodgeAction = value;
    }
    public Vector3 verticalVelocity => new Vector3(0f, m_Rigidbody.linearVelocity.y, 0f);
    public Vector3 horizontalVelocity => new Vector3(m_Rigidbody.linearVelocity.x, 0f, m_Rigidbody.linearVelocity.z);    

    #region Main Methods
    public T GetCurrentState<T>() where T : StateBase
    {
        return m_StateMachine.GetCurrentState<T>();
    }

    public bool IsCurrentState<T>() where T : StateBase
    {
        return m_StateMachine.IsCurrentState<T>();
    }    

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

    public void MakeDodgeAction(Vector2 input)
    {
        dodgeAction = ECharacterDodgeAction.Forward;

        if (input.x > 0.4f)
            dodgeAction = ECharacterDodgeAction.Right;
        else if (input.x < -0.4f)
            dodgeAction = ECharacterDodgeAction.Left;
        else if(input.y < -0.4f)
            dodgeAction = ECharacterDodgeAction.Backward;
    }

    public void PlayOneShot(AudioClip clip)
    {
        m_AudioPool?.PlayOneShot(clip);
    }

    public bool IsAnimationInTransition(int layer = 0)
    {
        return m_Model?.animator.IsInTransition(layer) ?? false;
    }

    protected void Init()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_CapsuleCollider = GetComponent<CapsuleCollider>();

        m_Sensor = GetComponent<CharacterSensor>();
        m_Sensor.Init(this);

        m_StateMachine = new StateMachine();
        m_StateMachine.Init(this);

        m_AttackComponent = GetComponent<AttackComponent>();
        m_AttackComponent?.Init(this);

        m_AudioPool = GetComponent<AudioPool>();        
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
    public int GUID => m_CharacterGUID;
    public Transform modelTransform => m_Model.transform;
    public StateMachine stateMachine => m_StateMachine;

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

    public virtual void OnTargetFind(Transform target)
    { 
    }

    public virtual void OnTargetLost(Transform target)
    { 
    }

    public virtual void OnTargetChange(Transform current, Transform last)
    { 
    }

    public virtual void OnTargetDistZoneChange(EDistanceZone newZone, EDistanceZone oldZone, float distance)
    { 
    }
    #endregion
}
