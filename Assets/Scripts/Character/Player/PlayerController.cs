using UnityEngine;

public class PlayerController : MonoBehaviour, IStateMachineOwner, ISkillOwner
{
    public PlayerConfig config = new PlayerConfig();
    [SerializeField] private PlayerAnimationConsts m_AnimationConsts;

    private PlayerAttrs m_Attrs = new PlayerAttrs();
    private StateMachine m_StateMachine;
    private Rigidbody m_Rigidbody;
    private CapsuleCollider m_CapsuleCollider;
    private PlayerModel m_PlayerModel;
    private AudioSource m_AudioSource;
    private ResizableCapsuleCollider m_ResizableCapsuleCollider;

    public AudioClip[] footStepAudioClips;
    public PlayerModel model { get => m_PlayerModel; }
    public PlayerAnimationConsts animConsts { get => m_AnimationConsts; }
    public Rigidbody rigidBody { get => m_Rigidbody; }
    public CapsuleCollider capsuleCollider { get => m_CapsuleCollider; }
    public ResizableCapsuleCollider resizableCapsule { get => m_ResizableCapsuleCollider; }
    public PlayerAttrs attrs { get => m_Attrs; }

    #region State Methods
    private void Awake()
    {
        m_PlayerModel = GetComponentInChildren<PlayerModel>();
        if (m_PlayerModel == null)
            throw new System.Exception("err, PlayerModel hasn't been asigned in children.");
        m_PlayerModel.Init(this);
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

        m_ResizableCapsuleCollider = gameObject.AddComponent<ResizableCapsuleCollider>();
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

    private void OnTriggerEnter(Collider other)
    {
        m_StateMachine.currentState?.HandleTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        m_StateMachine.currentState?.HandleTriggerExit(other);
    }
    #endregion

    #region Main Methods
    public void ChangeState(EPlayerState state, ChangeStateArgs args = default(ChangeStateArgs))
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
            case EPlayerState.JumpIdle:
                m_StateMachine?.ChangeState<PlayerStateJumpIdle>(args);
                break;
            case EPlayerState.Roll:
                m_StateMachine?.ChangeState<PlayerStateRoll>(args);
                break;
            case EPlayerState.Falling:
                m_StateMachine?.ChangeState<PlayerStateFalling>(args);
                break;
            case EPlayerState.Land:
                m_StateMachine?.ChangeState<PlayerStateLand>(args);
                break;
            case EPlayerState.LightAttack:
                m_StateMachine?.ChangeState<PlayerStateLightAttack>(args);
                break;
            default:
                break;
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

    public void OnAttack(ISkillTarget target, Vector3 hitPos)
    {
        target?.OnDamage(10);
    }
    #endregion
}
