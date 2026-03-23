using System;
using System.Collections;
using UnityEngine;

public class PlayerStateDefence : PlayerStateCombat
{
    /// <summary>
    /// Internal state of defence
    /// </summary>
    private enum EDefenceState : UInt16
    {
        Enter = 0,
        Loop,
        CounterAttackAWait,
        CounterAttackPerform,
        CounterAttackRunOut,
        End
    }

    private EDefenceState m_SubState;
    private Coroutine m_RestoreAttackCoroutine;

    #region State Methods
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        MonoManager.Stop(m_RestoreAttackCoroutine);
        m_SubState = EDefenceState.Enter;
        m_Player.model.StopAnimation(AnimationConsts.defenceRelease);
        m_Player.model.StartAnimation(AnimationConsts.defence);
        if (args.playMode == ChangeStateArgs.EAnimationPlayMode.Manual)
        {
            m_Player.model.StartAnimation(AnimationConsts.defenceState, 0.05f, AnimationConsts.BASE_LAYER);
        }
        AnimationEventReceiver.instance.RegisterAction(GUIDConsts.PlayerAnimation, AnimationEventType.AnimationTransit, OnDefenceEndTransition);
    }

    public override void Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveAction(GUIDConsts.PlayerAnimation, AnimationEventType.AnimationTransit, OnDefenceEndTransition);
        m_Player.model.StopAnimation(AnimationConsts.defence);
        base.Exit(newState);
    }

    public override void Update()
    {
        if (m_SubState == EDefenceState.CounterAttackAWait && m_Player.action.isLightAttack)
        {
            MonoManager.Stop(m_RestoreAttackCoroutine);
            m_SubState = EDefenceState.CounterAttackPerform;
            m_Player.ChangeState(ECharacterState.Attack);
            return;
        }

        if (!m_Player.action.holdDefence && m_SubState != EDefenceState.End)
        {
            m_SubState = EDefenceState.End;
            m_Player.model.StartAnimation(AnimationConsts.defenceRelease);            
            return;
        }
        else if(m_Player.action.holdDefence && m_SubState == EDefenceState.Enter && m_Player.model.animator.IsTransitToState("DefenceHold", AnimationConsts.BASE_LAYER))
        {
            m_SubState = EDefenceState.Loop;
            return;
        }        
    }    

    public override void FixedUpdate()
    {
        if (!m_Player.action.isMoving)
            return;

        Vector3 targetDir = m_Player.GetTargetDirection();
        m_Player.RotateToTargetDir(targetDir, m_Player.config.rotateSpeed);
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
