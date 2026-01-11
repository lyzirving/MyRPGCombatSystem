using UnityEngine;

public class PlayerController : MonoBehaviour, IStateMachineOwner
{
    public PlayerConfig config = new PlayerConfig();
    [SerializeField] private PlayerAnimationConsts m_AnimationConsts;    

    private PlayerAttrs m_Attrs = new PlayerAttrs();
    private StateMachine m_StateMachine;
    private CharacterController m_CharacterController;
    private PlayerModel m_PlayerModel;
    private AudioSource m_AudioSource;

    public PlayerModel model { get => m_PlayerModel; }
    public PlayerAnimationConsts animConsts { get => m_AnimationConsts; }
    public CharacterController character { get => m_CharacterController; }
    public PlayerAttrs attrs { get => m_Attrs; }
    public AudioClip[] footStepAudioClips;

    #region State Methods
    private void Awake()
    {
        m_PlayerModel = GetComponentInChildren<PlayerModel>();
        if (m_PlayerModel == null)
            throw new System.Exception("err, PlayerModel hasn't been asigned in children.");
        m_PlayerModel.RegisterLeftFootStepAction(OnLeftFootDown);
        m_PlayerModel.RegisterRightFootStepAction(OnRightFootDown);

        m_CharacterController = GetComponent<CharacterController>();
        if (m_CharacterController == null)
            throw new System.Exception("err, CharacterController hasn't been asigned.");

        m_AudioSource = GetComponent<AudioSource>();
        if (m_AudioSource == null)
            throw new System.Exception("err, AudioSource hasn't been asigned.");

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

    private void OnDisable()
    {
        m_PlayerModel.RemoveLeftFootStepAction(OnLeftFootDown);
        m_PlayerModel.RemoveRightFootStepAction(OnRightFootDown);
    }

    #endregion

    #region Main Methods
    public void ChangeState(PlayerState state, in ChangeStateArgs args = default(ChangeStateArgs))
    {
        m_Attrs.currentState = state;
        switch (state)
        {
            case PlayerState.Idle:
                m_StateMachine?.ChangeState<PlayerStateIdle>(args);
                break;
            case PlayerState.Walk:
                m_StateMachine?.ChangeState<PlayerStateWalk>(args);
                break;
            case PlayerState.Run:
                m_StateMachine?.ChangeState<PlayerStateRun>(args);
                break;
            case PlayerState.Jump:
                m_StateMachine?.ChangeState<PlayerStateJump>(args);
                break;
            case PlayerState.Falling:
                m_StateMachine?.ChangeState<PlayerStateFalling>(args);
                break;
            case PlayerState.Land:
                m_StateMachine?.ChangeState<PlayerStateLand>(args);
                break;
            case PlayerState.Stop:
                m_StateMachine?.ChangeState<PlayerStateStop>(args);
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
            case PlayerState.Jump:
            case PlayerState.Falling:
                return config.jumpMoveSpeed;
            case PlayerState.Idle:
            default:
                return 0f;
        }
    }

    public float GetGravityRatio()
    {
        switch (m_Attrs.currentState)
        {
            case PlayerState.Falling:
                return config.fallGravityRatio;
            default:
                return 1f;
        }
    }

    private void OnLeftFootDown()
    {
        if (footStepAudioClips == null || footStepAudioClips.Length == 0)
            return;

        m_AudioSource.PlayOneShot(footStepAudioClips[1]);
    }

    private void OnRightFootDown()
    {
        if (footStepAudioClips == null || footStepAudioClips.Length == 0)
            return;

        m_AudioSource.PlayOneShot(footStepAudioClips[1]);
    }
    #endregion
}
