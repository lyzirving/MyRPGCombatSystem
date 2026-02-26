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

    public CharacterAttrs attrs { get => m_Attrs; }
    public Rigidbody rigidBody { get => m_Rigidbody; }
    public CharacterSensor sensor { get => m_Sensor; }

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

    public void ResetVerticalVelocity()
    {
        m_Rigidbody.linearVelocity = horizontalVelocity;
    }

    protected void Init()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Sensor = this.transform.AddComponent<CharacterSensor>();

        m_StateMachine = new StateMachine();
        m_StateMachine.Init(this);
    }
    #endregion

    #region IStateMachineOwner Methods
    public virtual void ChangeState(ECharacterState state, ChangeStateArgs args = default(ChangeStateArgs))
    {        
    }
    #endregion

    #region ICharacterBehavior Methods
    public virtual bool isLightAttack => false;

    public virtual void OnAttackBegin() { }

    public virtual void OnAttackEnd() { }

    public virtual void OnAttackHit(SkillData config, ICharacterBehavior target, Vector3 hitPos) { }

    public virtual void OnDamage(float damage) { }

    public virtual void OnFootStep() { }
    #endregion

    //public void Floating()
    //{
    //    Vector3 centerInWorldSpace = m_ResizableCapsuleCollider.center;
    //    var ray = new Ray(centerInWorldSpace, -this.transform.up);
    //
    //    if (Physics.Raycast(ray, out RaycastHit hit, m_ResizableCapsuleCollider.slopeData.floatRayDistance, GameConsts.WalkableLayer, QueryTriggerInteraction.Ignore))
    //    {
    //        float groundAngle = Vector3.Angle(hit.normal, -ray.direction);
    //
    //        float distanceToFloatingPoint = m_ResizableCapsuleCollider.colliderData.centerInLocalSpace.y * this.transform.localScale.y - hit.distance;
    //        if (Mathf.Approximately(distanceToFloatingPoint, 0f))
    //            return;
    //
    //        float amountToLift = distanceToFloatingPoint * m_ResizableCapsuleCollider.slopeData.stepReachForce - verticalVelocity.y;
    //        Vector3 liftForce = new Vector3(0f, amountToLift, 0f);
    //        m_Rigidbody.AddForce(liftForce, ForceMode.VelocityChange);
    //    }
    //}
}
