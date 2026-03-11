using UnityEngine;

public class AIController : CharacterControllerBase
{
    // -------- Components in children start ------
    private AIModel m_AIModel;
    // -------- Components in children end --------

    public AIModel model { get => m_AIModel; }

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
            case ECharacterState.Hurt:
                m_StateMachine?.ChangeState<AIStateHurt>(args);
                break;
            default:
                break;
        }
    }
    #endregion

    #region ICharacterBehavior Methods
    public override void OnHit(Vector3 hitPos, float damage)
    {
        Debug.Log($"OnHit: {damage}");
        ChangeState(ECharacterState.Hurt, new ChangeStateArgs(true, hitPos));
    }
    #endregion
}
