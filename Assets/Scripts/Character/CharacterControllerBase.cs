using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CharacterControllerBase : MonoBehaviour, IStateMachineOwner, ICharacterBehavior
{
    protected CharacterAttrs m_Attrs = new CharacterAttrs();
    protected StateMachine m_StateMachine;

    protected Rigidbody m_Rigidbody;    
    protected CharacterSensor m_Sensor;
    protected CapsuleCollider m_CapsuleCollider;

    public CharacterAttrs attrs { get => m_Attrs; }
    public Rigidbody rigidBody { get => m_Rigidbody; }
    public CharacterSensor sensor { get => m_Sensor; }
    public CapsuleCollider capsule => m_CapsuleCollider;

    public Vector3 verticalVelocity => new Vector3(0f, m_Rigidbody.linearVelocity.y, 0f);
    public Vector3 horizontalVelocity => new Vector3(m_Rigidbody.linearVelocity.x, 0f, m_Rigidbody.linearVelocity.z);
    public bool isMovingUp => m_Rigidbody.linearVelocity.y > 0f;
    public bool isMoveHorizontally => horizontalVelocity.magnitude > Mathf.Epsilon;
    public Vector3 cameraDirection => new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z).normalized;
    public Quaternion cameraRotation => Quaternion.Euler(new Vector3(0f, Camera.main.transform.eulerAngles.y, 0f));

    #region Main Methods
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
    #endregion

    #region ICharacterBehavior Methods
    public virtual bool isLightAttack => false;

    public virtual Transform modelTransform => null;

    public virtual void OnAttackBegin() { }

    public virtual void OnAttackEnd() { }

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
