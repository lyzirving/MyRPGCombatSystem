using UnityEngine;

public class PlayerController : MonoBehaviour, IStateMachineOwner
{
    public PlayerConfig config = new PlayerConfig();
    [SerializeField] private PlayerAnimationConsts m_AnimationConsts;    

    private PlayerAttrs m_Attrs = new PlayerAttrs();
    private StateMachine m_StateMachine;
    private Rigidbody m_Rigidbody;
    private CapsuleCollider m_CapsuleCollider;
    private PlayerModel m_PlayerModel;
    private AudioSource m_AudioSource;

    public AudioClip[] footStepAudioClips;
    public PlayerModel model { get => m_PlayerModel; }
    public PlayerAnimationConsts animConsts { get => m_AnimationConsts; }
    public Rigidbody rigidBody { get => m_Rigidbody; }
    public CapsuleCollider capsuleCollider { get => m_CapsuleCollider; }
    public PlayerAttrs attrs { get => m_Attrs; }    

    #region State Methods
    private void Awake()
    {
        m_PlayerModel = GetComponentInChildren<PlayerModel>();
        if (m_PlayerModel == null)
            throw new System.Exception("err, PlayerModel hasn't been asigned in children.");
        m_PlayerModel.RegisterLeftFootStepAction(OnLeftFootDown);
        m_PlayerModel.RegisterRightFootStepAction(OnRightFootDown);

        m_Rigidbody = GetComponent<Rigidbody>();
        if (m_Rigidbody == null)
            throw new System.Exception("err, Rigidbody hasn't been asigned.");

        m_CapsuleCollider = GetComponent<CapsuleCollider>();
        if (m_CapsuleCollider == null)
            throw new System.Exception("err, CapsuleCollider hasn't been asigned.");

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
        ChangeState(EPlayerState.Idle);
    }

    private void OnDisable()
    {
        m_PlayerModel.RemoveLeftFootStepAction(OnLeftFootDown);
        m_PlayerModel.RemoveRightFootStepAction(OnRightFootDown);
    }

    #endregion

    #region Main Methods
    public void ChangeState(EPlayerState state, in ChangeStateArgs args = default(ChangeStateArgs))
    {
        m_Attrs.currentState = state;
        switch (state)
        {
            case EPlayerState.Idle:
                m_StateMachine?.ChangeState<PlayerStateIdle>(args);
                break;
            case EPlayerState.Walk:
                m_StateMachine?.ChangeState<PlayerStateWalk>(args);
                break;
            case EPlayerState.Run:
                m_StateMachine?.ChangeState<PlayerStateRun>(args);
                break;
            case EPlayerState.Jump:
                m_StateMachine?.ChangeState<PlayerStateJump>(args);
                break;
            case EPlayerState.Falling:
                m_StateMachine?.ChangeState<PlayerStateFalling>(args);
                break;
            case EPlayerState.Land:
                m_StateMachine?.ChangeState<PlayerStateLand>(args);                
                break;
            default:
                break;
        }
    }

    public float GetMoveSpeed()
    {
        switch (m_Attrs.currentState)
        {
            case EPlayerState.Walk:
                return config.walkSpeed;
            case EPlayerState.Run:
                return config.runSpeed;
            case EPlayerState.Jump:
            case EPlayerState.Falling:
                return config.jumpMoveSpeed;
            case EPlayerState.Idle:
            default:
                return 0f;
        }
    }

    public float GetGravityRatio()
    {
        switch (m_Attrs.currentState)
        {
            case EPlayerState.Falling:
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
