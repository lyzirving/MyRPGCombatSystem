using System;
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
        End
    }

    private EDefenceState m_State;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_State = EDefenceState.Enter;
        m_Player.model.StopAnimation(AnimationConsts.defenceRelease);
        m_Player.model.StartAnimation(AnimationConsts.defence);
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
        if (!m_Player.action.holdDefence && m_State != EDefenceState.End)
        {
            m_Player.model.StartAnimation(AnimationConsts.defenceRelease);
            m_State = EDefenceState.End;
            return;
        }
        else if(m_Player.action.holdDefence && m_State == EDefenceState.Enter && m_Player.model.animator.IsTransitToState("DefenceHold", AnimationConsts.BASE_LAYER))
        {
            m_State = EDefenceState.Loop;
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

    private void OnDefenceEndTransition(in AnimationEventInfo info)
    {
        m_Player.ChangeState(ECharacterState.Idle);
    }
}
