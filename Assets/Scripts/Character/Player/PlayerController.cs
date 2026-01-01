using UnityEngine;

public class PlayerController : MonoBehaviour, IStateMachineOwner
{
    public PlayerConfig config = new PlayerConfig();
    [SerializeField] private PlayerAnimationConsts m_AnimationConsts;    

    private StateMachine m_StateMachine;
    private CharacterController m_CharacterController;
    private PlayerModel m_PlayerModel;

    public PlayerModel model { get => m_PlayerModel; }
    public PlayerAnimationConsts animConsts { get => m_AnimationConsts; }
    public CharacterController character { get => m_CharacterController; }

    #region State Methods
    private void Awake()
    {
        m_PlayerModel = GetComponentInChildren<PlayerModel>();
        if (m_PlayerModel == null)
            throw new System.Exception("err, PlayerModel hasn't been asigned in children.");

        m_CharacterController = GetComponent<CharacterController>();
        if (m_CharacterController == null)
            throw new System.Exception("err, CharacterController hasn't been asigned.");

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
            case PlayerState.Move:
                m_StateMachine?.ChangeState<PlayerStateMove>();
                break;
            default:
                break;
        }
    }
    #endregion
}
