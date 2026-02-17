using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CharacterControllerBase : MonoBehaviour, IStateMachineOwner, ICharacterBehavior
{
    protected CharacterAttrs m_Attrs = new CharacterAttrs();
    protected Rigidbody m_Rigidbody;
    protected ResizableCapsuleCollider m_ResizableCapsuleCollider;
    protected StateMachine m_StateMachine;

    public CharacterAttrs attrs { get => m_Attrs; }
    public Rigidbody rigidBody { get => m_Rigidbody; }
    public ResizableCapsuleCollider resizableCapsule { get => m_ResizableCapsuleCollider; }

    #region Main Methods
    protected void Init()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_ResizableCapsuleCollider = gameObject.AddComponent<ResizableCapsuleCollider>();

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

    public virtual void OnAttackBegin()
    {        
    }

    public virtual void OnAttackEnd()
    {
    }

    public virtual void OnAttackHit(SkillData config, ICharacterBehavior target, Vector3 hitPos)
    {
    }

    public virtual void OnDamage(float damage)
    {
    }

    public virtual void OnFootStep()
    {
    }    
    #endregion
}
