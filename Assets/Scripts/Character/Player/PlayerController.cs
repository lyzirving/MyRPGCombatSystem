using UnityEngine;

public class PlayerController : MonoBehaviour, IStateMachineOwner
{
    public PlayerConfig config = new PlayerConfig();
    [SerializeField] private PlayerAnimationConsts m_AnimationConsts;

    private PlayerAttrs m_Attrs = new PlayerAttrs();
    private StateMachine m_StateMachine;
    private CharacterController m_CharacterController;
    private PlayerModel m_PlayerModel;

    public PlayerModel model { get => m_PlayerModel; }
    public PlayerAnimationConsts animConsts { get => m_AnimationConsts; }
    public CharacterController character { get => m_CharacterController; }
    public PlayerAttrs attrs { get => m_Attrs; }

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

        InputManager.instance.runToggleChange += OnRunToggleChange;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        ChangeState(PlayerState.Idle);
    }

    private void OnDestroy()
    {
        InputManager.instance.runToggleChange -= OnRunToggleChange;
    }
    #endregion

    #region Main Methods
    public void ChangeState(PlayerState state)
    {
        m_Attrs.currentState = state;
        switch (state)
        {
            case PlayerState.Idle:
                m_StateMachine?.ChangeState<PlayerStateIdle>();
                break;
            case PlayerState.Walk:
                m_StateMachine?.ChangeState<PlayerStateWalk>();
                break;
            case PlayerState.Run:
                m_StateMachine?.ChangeState<PlayerStateRun>();
                break;
            default:
                break;
        }
    }

    public float GetMoveSpeed()
    {
        switch (m_Attrs.currentState)
        {
            case PlayerState.Walk:
                return config.walkSpeed;
            case PlayerState.Run:
                return config.runSpeed;
            case PlayerState.Idle:
            default:
                return 0f;
        }
    }

    private void OnRunToggleChange(bool shouldRun)
    {
        if (shouldRun && m_Attrs.currentState == PlayerState.Walk)
        {
            ChangeState(PlayerState.Run);
        }
        else if (!shouldRun && m_Attrs.currentState == PlayerState.Run)
        {
            ChangeState(PlayerState.Walk);
        }
    }
    #endregion
}
