using UnityEngine;

public class PlayerController : CharacterControllerBase
{
    public PlayerModel model => m_PlayerModel;
    public PlayerActionController action => m_ActionController;
    public GhostTrail ghostTrail => m_GhostTrail;

    // -------- Component in current node start --------
    private PlayerActionController m_ActionController;
    private GhostTrail m_GhostTrail;
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

        m_GhostTrail = GetComponent<GhostTrail>();

        m_ActionController = GetComponent<PlayerActionController>();      

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
            case ECharacterState.StrafeMove:
                m_StateMachine?.ChangeState<PlayerStateStrafeMove>(args);
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
            case ECharacterState.Dodge:
                m_StateMachine?.ChangeState<PlayerStateDodge>(args);
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
        PlayOneShot(m_AttackComponent.skill.skillReleaseData.audioClip);
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
        PlayOneShot(config.footstep.runningAudio);
    }

    public override void OnHit(Vector3 hitPos, in ICharacterBehavior source, in SkillData skillData)
    {
        var defenceState = m_StateMachine.GetCurrentState<PlayerStateDefence>();
        if (defenceState != null)
        {
            //TODO: add config for counter attack window
            defenceState.OnHit(0.2f);
        }
    }

    public override void OnTargetFind(Transform target)
    {
        lockTarget = target;
        m_PlayerModel.SetAnimationBool(AnimationConsts.locked, true);
    }

    public override void OnTargetLost(Transform target)
    {
        lockTarget = null;
        m_PlayerModel.SetAnimationBool(AnimationConsts.locked, false);
        if (IsCurrentState<PlayerStateStrafeMove>())
            ChangeState(ECharacterState.Move);
    }

    public override void OnTargetChange(Transform current, Transform last)
    {
        lockTarget = current;
        if (lockTarget != null)
            m_PlayerModel.SetAnimationBool(AnimationConsts.locked, true);
    }
    #endregion
}
