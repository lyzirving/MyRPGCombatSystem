using UnityEngine;

public class AIController : CharacterControllerBase
{
    #region State Methods
    private void Awake()
    {
        base.Init();

        m_Model = GetComponentInChildren<AIModel>();
        m_Model.Init(this);

        AIManager.instance.Register(this);
    }

    private void Start()
    {
        ChangeState(ECharacterState.Idle);
    }
    #endregion

    #region IStateMachineOwner Methods
    public override void ChangeState(ECharacterState state, ChangeStateArgs args = default(ChangeStateArgs))
    {
        switch (state)
        {
            case ECharacterState.Idle:
                m_StateMachine?.ChangeState<AIStateIdle>(args);                
                break;
            case ECharacterState.Move:
                m_StateMachine?.ChangeState<AIStateMove>(args);
                break;            
            case ECharacterState.Roar:
                m_StateMachine?.ChangeState<AIStateRoar>(args);
                break;
            case ECharacterState.Defence:
                m_StateMachine?.ChangeState<AIStateDefence>(args);
                break;
            case ECharacterState.Attack:
                m_StateMachine?.ChangeState<AIStateAttack>(args);
                break;
            default:
                break;
        }
    }

    public override void AddAdditiveState(ECharacterState state, ChangeStateArgs args = default)
    {
        switch (state)
        {
            case ECharacterState.Hurt:
                m_StateMachine?.AddAdditive<AIStateHurt>(args);
                break;
            default:
                break;
        }
    }

    public override void RemoveAdditiveState(ECharacterState state)
    {
        switch (state)
        {
            case ECharacterState.Hurt:
                m_StateMachine?.RemoveAdditive<AIStateHurt>();
                break;
            default:
                break;
        }
    }
    #endregion

    #region ICharacterBehavior Methods
    public override void OnHit(Vector3 hitPos, in ICharacterBehavior source, in SkillData skillData)
    {
        AddAdditiveState(ECharacterState.Hurt, new ChangeStateArgs(source, skillData, hitPos));
    }
    #endregion
}
