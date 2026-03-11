using UnityEngine;

public class PlayerController : CharacterControllerBase
{
    public PlayerConfig config = new PlayerConfig();
    public AudioClip[] footStepAudioClips;

    public float movementSpeed => config.baseSpeed * m_Attrs.speedModify;
    public PlayerModel model { get => m_PlayerModel; }
    public PlayerActionController action { get => m_ActionController; }
    public AttackComponent attackComponent { get => m_AttackComponent; } 

    // -------- Component in current node start --------
    private PlayerActionController m_ActionController;
    private AudioPool m_AudioPool;
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
        m_AudioPool = GetComponent<AudioPool>();

        m_AttackComponent = GetComponent<AttackComponent>();
        m_AttackComponent.Init(this);

        // Init components in children
        m_PlayerModel = GetComponentInChildren<PlayerModel>();
        m_PlayerModel.Init(this);        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        ChangeState(ECharacterState.Idle);
    }
    #endregion

    #region Main Methods
    public Vector3 GetTargetDirection()
    {
        if (!m_ActionController.isMoving)
            return this.transform.forward;

        return cameraRotation * m_ActionController.GetInputDirection();
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
            case ECharacterState.Move:
                m_StateMachine?.ChangeState<PlayerStateMove>(args);
                break;
            case ECharacterState.Jump:
                m_StateMachine?.ChangeState<PlayerStateJump>(args);
                break;
            case ECharacterState.Falling:
                m_StateMachine?.ChangeState<PlayerStateFall>(args);
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
        m_AudioPool?.PlayOneShot(m_AttackComponent.skill.skillReleaseData.audioClip);
        m_AttackComponent.attackBox.OnAttackBegin();
    }

    public override void OnAttackEnd()
    {
        m_AttackComponent.attackBox.OnAttackEnd();
    }

    public override void OnAttackHit(ICharacterBehavior target, Vector3 hitPos)
    {
        m_PlayerModel.HitStop(m_AttackComponent.skill.skillHitData.hitStopTimeScale);
        target?.OnHit(hitPos, m_AttackComponent.skill.damage);
        VFXManager.instance.Play(m_AttackComponent.skill.skillHitData.spawnPrefab, hitPos, Quaternion.identity);
    }

    public override void OnFootStep(EFootstep footStep)
    {
        if (footStepAudioClips == null || footStepAudioClips.Length == 0)
            return;

        m_AudioPool?.PlayOneShot(footStepAudioClips[1]);
    }
    #endregion
}
