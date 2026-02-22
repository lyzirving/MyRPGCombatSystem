using UnityEngine;

public class AIController : CharacterControllerBase
{
    public int hitLayerIndex = -1;
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
        hitLayerIndex = m_AIModel.GetLayerIndex(AnimationConsts.hitLayer);

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
            case ECharacterState.Hit:
                m_StateMachine?.ChangeState<AIStateHit>(args);
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
        ChangeState(ECharacterState.Hit);
    }
    #endregion
}
