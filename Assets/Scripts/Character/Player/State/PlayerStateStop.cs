using System;
using UnityEngine;

public class PlayerStateStop : PlayerStateMove
{
    private int m_AnimHash;

    public override void Enter(StateBase exitState, in ChangeStateArgs args)
    {
        m_AnimHash = (args.footStep == EFootStep.LeftFoot) ? m_Player.animConsts.rightFootStopHash : m_Player.animConsts.leftFootStopHash;
        base.Enter(exitState, args);
        Debug.Log($"foot step is [{args.footStep}]");
        m_Player.model.StopAnimation(m_Player.animConsts.walkHash);
        m_Player.model.StartAnimation(m_AnimHash);
        m_Player.model.RegisterRootMotionAction(HandleRootMotion);
        AnimationEventReceiver.instance.RegisterHandler(AnimationDefine.PlayerAnimationEvent.RunStopFinish, HandleStopFinish);
    }    

    public override void Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveHandler(AnimationDefine.PlayerAnimationEvent.RunStopFinish, HandleStopFinish);
        m_Player.model.RemoveRootMotionAction(HandleRootMotion);
        m_Player.model.StopAnimation(m_AnimHash);
        base.Exit(newState);
    }

    public override void Update()
    {   
        // Must be overrided and should do nothing.
    }

    private void HandleRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        deltaPosition.y = 0f;
        m_Player.character.Move(deltaPosition);
    }

    private void HandleStopFinish()
    {
        if (!InputManager.instance.isPlayerMoving)
        {
            m_Player.ChangeState(PlayerState.Idle);
        }
        else
        {
            m_Player.ChangeState(InputManager.instance.shouldPlayerRun ? PlayerState.Run : PlayerState.Walk);
        }
    }
}
