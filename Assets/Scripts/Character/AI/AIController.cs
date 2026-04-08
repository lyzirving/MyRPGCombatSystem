using UnityEngine;

public class AIController : CharacterControllerBase
{
    // -------- Components in children start ------
    private AIModel m_AIModel;
    // -------- Components in children end --------

    public AIModel model { get => m_AIModel; }

    #region Override Virtual Methods
    public override bool IsInAnimationTransition(int layer = 0)
    {
        return m_AIModel.animator.IsInTransition(layer);
    }
    #endregion

    #region State Methods
    private void Awake()
    {
        base.Init();

        // Init components in children
        m_AIModel = GetComponentInChildren<AIModel>();
        m_AIModel.Init(this);
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
    public override Transform modelTransform { get => m_AIModel.transform; }

    public override void OnHit(Vector3 hitPos, in ICharacterBehavior source, in SkillData skillData)
    {
        AddAdditiveState(ECharacterState.Hurt, new ChangeStateArgs(true, source, skillData, hitPos));
    }
    #endregion
}
