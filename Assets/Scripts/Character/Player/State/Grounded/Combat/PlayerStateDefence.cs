using System.Collections;
using UnityEngine;

public class PlayerStateDefence : PlayerStateCombat
{
    private EDefenceState m_SubState;
    private Coroutine m_RestoreAttackCoroutine;

    #region State Methods
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        MonoManager.Stop(m_RestoreAttackCoroutine);
        m_SubState = EDefenceState.Enter;
        m_Player.model.SetAnimationBool(AnimationConsts.defenceRelease, false);
        m_Player.model.SetAnimationBool(AnimationConsts.defence, true);
        if (args.playMode == ChangeStateArgs.EAnimationPlayMode.Manual)
        {
            m_Player.model.StartAnimation(AnimationConsts.defenceState, 0.05f, AnimationConsts.BASE_LAYER);
        }
        AnimationEventReceiver.instance.RegisterAction(GUIDConsts.PlayerAnimation, AnimationEventType.AnimationTransit, OnDefenceEndTransition);
    }

    public override bool Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveAction(GUIDConsts.PlayerAnimation, AnimationEventType.AnimationTransit, OnDefenceEndTransition);
        m_Player.model.SetAnimationBool(AnimationConsts.defence, false);
        base.Exit(newState);
        return true;
    }

    public override bool HandleInput()
    {
        return false;
    }

    public override void Update()
    {
        if (m_Player.action.isDefenceHolding && m_SubState == EDefenceState.Enter && m_Player.model.animator.IsTransitToState("DefenceHold", AnimationConsts.BASE_LAYER))
        {
            m_SubState = EDefenceState.Loop;
            return;
        }
        else if (!m_Player.action.isDefenceHolding && m_SubState != EDefenceState.End)
        {
            m_SubState = EDefenceState.End;
            m_Player.model.SetAnimationBool(AnimationConsts.defenceRelease, true);            
            return;
        }     
    }    

    public override void FixedUpdate()
    {
        if (!m_Player.action.isMoving)
            return;

        Vector3 targetDir = m_Player.GetTargetDirection();
        m_Player.RotateToTargetDir(targetDir, m_Player.config.move.rotateSpeed);
    }

    public override bool CanExecute(ECharacterAction action)
    {
        if (action == ECharacterAction.LightAttack)
            return m_SubState == EDefenceState.CounterAttackAWait;

        return true;
    }

    public override void Execute(ECharacterAction action)
    {
        switch (action)
        {
            case ECharacterAction.LightAttack:
                MonoManager.Stop(m_RestoreAttackCoroutine);
                m_SubState = EDefenceState.CounterAttackPerform;
                m_Player.ChangeState(ECharacterState.Attack);
                return;
            default:
                break;
        }
    }

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Defence;
    }
    #endregion    

    #region Animation Event Handle
    private void OnDefenceEndTransition(in AnimationEventInfo info)
    {
        m_Player.ChangeState(ECharacterState.Idle);
    }
    #endregion

    public void OnHit(float attachWindowTime)
    {
        MonoManager.Stop(m_RestoreAttackCoroutine);
        m_SubState = EDefenceState.CounterAttackAWait;
        m_RestoreAttackCoroutine = MonoManager.Run(RestoreCounterAttackState(attachWindowTime));       
    }

    private IEnumerator RestoreCounterAttackState(float attackWindowTime)
    { 
        float startTime = Time.time;
        while(Time.time - startTime < attackWindowTime)
            yield return null;

        m_SubState = EDefenceState.CounterAttackRunOut;
    }
}
