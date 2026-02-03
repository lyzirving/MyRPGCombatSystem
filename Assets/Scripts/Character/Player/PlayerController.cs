using UnityEngine;

public class PlayerController : MonoBehaviour, IStateMachineOwner, IPlayerBehavior
{
    public PlayerConfig config = new PlayerConfig();
    [SerializeField] private PlayerAnimationConsts m_AnimationConsts;
    public AudioClip[] footStepAudioClips;       
    
    public PlayerModel model { get => m_PlayerModel; }
    public PlayerAnimationConsts animConsts { get => m_AnimationConsts; }
    public Rigidbody rigidBody { get => m_Rigidbody; }
    public CapsuleCollider capsuleCollider { get => m_CapsuleCollider; }
    public ResizableCapsuleCollider resizableCapsule { get => m_ResizableCapsuleCollider; }
    public PlayerAttrs attrs { get => m_Attrs; }
    public PlayerActionController action { get => m_ActionController; }
    public AttackComponent attackComponent { get => m_AttackComponent; }

    private PlayerAttrs m_Attrs = new PlayerAttrs();
    private StateMachine m_StateMachine;    

    // -------- Component in current start --------
    private Rigidbody m_Rigidbody;
    private CapsuleCollider m_CapsuleCollider;
    private ResizableCapsuleCollider m_ResizableCapsuleCollider;
    private PlayerActionController m_ActionController;
    private AudioSource m_AudioSource;
    // -------- Component in current end --------

    // -------- Components in children start ------
    private PlayerModel m_PlayerModel;
    private AttackComponent m_AttackComponent;
    // -------- Components in children end ------

    #region State Methods
    private void Awake()
    {
        m_ActionController = GetComponent<PlayerActionController>();        
        m_Rigidbody = GetComponent<Rigidbody>();
        if (m_Rigidbody == null)
            throw new System.Exception("err, Rigidbody hasn't been asigned.");

        m_CapsuleCollider = GetComponent<CapsuleCollider>();
        if (m_CapsuleCollider == null)
            throw new System.Exception("err, CapsuleCollider hasn't been asigned.");

        m_AudioSource = GetComponent<AudioSource>();
        if (m_AudioSource == null)
            throw new System.Exception("err, AudioSource hasn't been asigned.");

        m_AttackComponent = GetComponent<AttackComponent>();
        if (m_AttackComponent == null)
            throw new System.Exception("err, PlayerAttackComponent hasn't been asigned.");
        m_AttackComponent.Init(this);

        m_ResizableCapsuleCollider = gameObject.AddComponent<ResizableCapsuleCollider>();
        m_StateMachine = new StateMachine();
        m_AnimationConsts = new PlayerAnimationConsts();

        m_StateMachine.Init(this);
        m_AnimationConsts.Init();

        // Init components in children
        m_PlayerModel = GetComponentInChildren<PlayerModel>();
        if (m_PlayerModel == null)
            throw new System.Exception("err, PlayerModel hasn't been asigned in children.");
        m_PlayerModel.Init(this);
        m_PlayerModel.RegisterLeftFootStepAction(OnLeftFootDown);
        m_PlayerModel.RegisterRightFootStepAction(OnRightFootDown);        
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
            case EPlayerState.StandardAttack:
                m_StateMachine?.ChangeState<PlayerStateStandardAttack>(args);
                break;
            default:
                break;
        }
    }

    private void OnLeftFootDown()
    {
        OnFootStep();
    }

    private void OnRightFootDown()
    {
        OnFootStep();
    }
    #endregion

    #region IPlayerBehaviour
    public void OnStartAttack(SkillData config)
    {
    }    

    public void OnAttackHit(SkillData config, ISkillTarget target, Vector3 hitPos)
    {
        target?.OnDamage(10);
    }

    public void OnStopAttack(SkillData config)
    {
    }

    public void OnFootStep()
    {
        if (footStepAudioClips == null || footStepAudioClips.Length == 0)
            return;

        m_AudioSource.PlayOneShot(footStepAudioClips[1]);
    }
    #endregion
}
