using UnityEngine;
using static UnityEngine.Rendering.STP;

public class PlayerController : CharacterControllerBase
{
    public PlayerConfig config = new PlayerConfig();
    [SerializeField] private PlayerAnimationConsts m_AnimationConsts;
    public AudioClip[] footStepAudioClips;

    public float movementSpeed => config.baseSpeed * m_Attrs.speedModify;
    public PlayerModel model { get => m_PlayerModel; }
    public PlayerAnimationConsts animConsts { get => m_AnimationConsts; }
    public PlayerActionController action { get => m_ActionController; }
    public AttackComponent attackComponent { get => m_AttackComponent; } 

    // -------- Component in current node start --------
    private PlayerActionController m_ActionController;
    private AudioSource m_AudioSource;
    private AttackComponent m_AttackComponent;
    // -------- Component in current node end --------

    // -------- Components in children start ------
    private PlayerModel m_PlayerModel;    
    // -------- Components in children end ------

    #region State Methods
    private void Awake()
    {
        base.Init();

        m_ActionController = GetComponent<PlayerActionController>();
        m_AudioSource = GetComponent<AudioSource>();

        m_AttackComponent = GetComponent<AttackComponent>();
        m_AttackComponent.Init(this);

        m_AnimationConsts = new PlayerAnimationConsts();
        m_AnimationConsts.Init();

        // Init components in children
        m_PlayerModel = GetComponentInChildren<PlayerModel>();
        m_PlayerModel.Init(this);
        m_PlayerModel.RegisterLeftFootStepAction(OnLeftFootDown);
        m_PlayerModel.RegisterRightFootStepAction(OnRightFootDown);        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        ChangeState(ECharacterState.Idle);
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
    public Vector3 GetTargetDirection()
    {
        if (!m_ActionController.isMoving)
            return this.transform.forward;

        return cameraRotation * m_ActionController.GetInputDirection();
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

    #region IStateMachineOwner Methods
    public override void ChangeState(ECharacterState state, ChangeStateArgs args = default(ChangeStateArgs))
    {
        switch (state)
        {
            case ECharacterState.Idle:
                m_StateMachine?.ChangeState<PlayerStateIdle>(args);
                break;
            case ECharacterState.Walk:
                m_StateMachine?.ChangeState<PlayerStateWalk>(args);
                break;
            case ECharacterState.Run:
                m_StateMachine?.ChangeState<PlayerStateRun>(args);
                break;
            case ECharacterState.Jump:
                m_StateMachine?.ChangeState<PlayerStateJump>(args);
                break;
            case ECharacterState.JumpIdle:
                m_StateMachine?.ChangeState<PlayerStateJumpIdle>(args);
                break;
            case ECharacterState.Roll:
                m_StateMachine?.ChangeState<PlayerStateRoll>(args);
                break;
            case ECharacterState.Falling:
                m_StateMachine?.ChangeState<PlayerStateFalling>(args);
                break;
            case ECharacterState.Land:
                m_StateMachine?.ChangeState<PlayerStateLand>(args);
                break;
            case ECharacterState.Attack:
                m_StateMachine?.ChangeState<PlayerStateAttack>(args);
                break;
            default:
                break;
        }
    }
    #endregion

    #region ICharacterBehavior Methods
    public override bool isLightAttack { get => m_ActionController.isLightAttack; }

    public override void OnAttackBegin()
    {
        m_AttackComponent.attackBox.OnAttackBegin();
    }

    public override void OnAttackEnd()
    {
        m_AttackComponent.attackBox.OnAttackEnd();
    }

    public override void OnAttackHit(SkillData config, ICharacterBehavior target, Vector3 hitPos)
    {
        target?.OnDamage(config.damage);
    }

    public override void OnFootStep()
    {
        if (footStepAudioClips == null || footStepAudioClips.Length == 0)
            return;

        m_AudioSource.PlayOneShot(footStepAudioClips[1]);
    }
    #endregion
}
