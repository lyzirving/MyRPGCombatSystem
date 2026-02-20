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
            default:
                break;
        }
    }
    #endregion

    #region ICharacterBehavior Methods
    public override void OnDamage(float damage)
    {
        Debug.LogWarning($"OnDamage: {damage}");
    }
    #endregion
}
