using UnityEngine;

public class PlayerController : MonoBehaviour, IStateMachineOwner
{
    [SerializeField] private PlayerModel m_PlayerModel;
    [SerializeField] private PlayerAnimationConsts m_AnimationConsts;    

    private StateMachine m_StateMachine;

    public PlayerModel model { get => m_PlayerModel; }
    public PlayerAnimationConsts animConsts { get => m_AnimationConsts; }    

    #region State Methods
    private void Awake()
    {
        if (m_PlayerModel == null)
            throw new System.Exception("Err, PlayerModel hasn't been asigned.");

        m_StateMachine = new StateMachine();        
        m_AnimationConsts = new PlayerAnimationConsts();

        m_StateMachine.Init(this);
        m_AnimationConsts.Init();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        ChangeState(PlayerState.Idle);
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
    #endregion

    #region Main Methods
    public void ChangeState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                m_StateMachine?.ChangeState<PlayerStateIdle>();
                break;
            default:
                break;
        }
    }
    #endregion
}
