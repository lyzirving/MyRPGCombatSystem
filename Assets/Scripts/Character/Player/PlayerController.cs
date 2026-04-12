using UnityEngine;

public class PlayerController : CharacterControllerBase
{    
    public AudioClip[] footStepAudioClips;

    public PlayerModel model { get => m_PlayerModel; }
    public PlayerActionController action { get => m_ActionController; }    

    // -------- Component in current node start --------
    private PlayerActionController m_ActionController;
    private AudioPool m_AudioPool;
    // -------- Component in current node end --------

    // -------- Components in children start ------
    private PlayerModel m_PlayerModel;
    // -------- Components in children end --------

    #region Override Virtual Methods
    public override bool IsInAnimationTransition(int layer = 0)
    {
        return m_PlayerModel.animator.IsInTransition(layer);
    }
    #endregion

    #region State Methods
    private void Awake()
    {
        base.Init();

        m_ActionController = GetComponent<PlayerActionController>();
        m_AudioPool = GetComponent<AudioPool>();        

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
            case ECharacterState.Defence:
                m_StateMachine?.ChangeState<PlayerStateDefence>(args);
                break;
            default:
                break;
        }
    }
    #endregion

    #region ICharacterBehavior Methods
    public override bool isLightAttack { get => m_ActionController.isLightAttack; }

    public override Transform modelTransform { get => m_PlayerModel.transform; }

    public override void OnAttackBegin()
    {
        m_AudioPool?.PlayOneShot(m_AttackComponent.skill.skillReleaseData.audioClip);
        base.OnAttackBegin();
    }

    public override void OnAttackHit(ICharacterBehavior target, Vector3 hitPos)
    {
        m_PlayerModel.HitStop(m_AttackComponent.skill.skillHitData.hitStopTimeScale);
        target?.OnHit(hitPos, this, m_AttackComponent.skill);
        VFXManager.instance.Play(m_AttackComponent.skill.skillHitData.spawnPrefab, hitPos, Quaternion.identity);
    }

    public override void OnFootStep(EFootstep footStep)
    {
        if (footStepAudioClips == null || footStepAudioClips.Length == 0)
            return;

        m_AudioPool?.PlayOneShot(footStepAudioClips[1]);
    }

    public override void OnHit(Vector3 hitPos, in ICharacterBehavior source, in SkillData skillData)
    {
        if (m_StateMachine.currentState is PlayerStateDefence)
        {
            var defenceState = m_StateMachine.currentState as PlayerStateDefence;
            //TODO: add config for counter attack window
            defenceState.OnHit(0.2f);
        }
    }
    #endregion
}
