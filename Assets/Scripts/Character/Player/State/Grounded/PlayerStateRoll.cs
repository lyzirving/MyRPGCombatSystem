using UnityEngine;

public class PlayerStateRoll : PlayerStateMove
{
    private Vector3 m_Direction;
    private bool m_IsRollTransit;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.animConsts.rollHash);
        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.RollTransit, HandleRollTransit);

        m_IsRollTransit = false;
        m_Player.attrs.speedModify = m_Player.config.rollSpeedModify;
        m_Direction = GetTargetDirection();
    }

    public override void Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveAction(AnimationEventType.RollTransit, HandleRollTransit);
        m_Player.model.StopAnimation(m_Player.animConsts.rollHash);
        base.Exit(newState);
    }

    public override void Update()
    {
        if(!m_IsRollTransit)
            return;

        if (!InputManager.instance.isPlayerMoving)
        {
            m_Player.ChangeState(EPlayerState.Idle);
        }
        else
        {
            m_Player.ChangeState(InputManager.instance.shouldPlayerRun ? EPlayerState.Run : EPlayerState.Walk);
        }
    }

    public override void FixedUpdate()
    {
        if (m_IsRollTransit)
            return;

        Float();

        MoveAt(m_Direction);
    }

    private void HandleRollTransit(AnimationEventInfo info)
    {
        m_IsRollTransit = true;
    }
}
