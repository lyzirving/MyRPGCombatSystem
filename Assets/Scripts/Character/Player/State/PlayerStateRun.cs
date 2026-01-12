using UnityEngine;

public class PlayerStateRun : PlayerStateMove
{
    private EFootStep m_FootStep = EFootStep.None;

    public override void Enter(StateBase exitState, in ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.animConsts.runHash);
        m_Player.model.RegisterLeftFootStepAction(OnLeftFootDown);
        m_Player.model.RegisterRightFootStepAction(OnRightFootDown);
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.RemoveLeftFootStepAction(OnLeftFootDown);
        m_Player.model.RemoveRightFootStepAction(OnRightFootDown);
        m_Player.model.StopAnimation(m_Player.animConsts.runHash); 
        base.Exit(newState);
    }

    public override void Update()
    {
        if (!CheckPlayerOnGround())
        {
            m_Player.ChangeState(PlayerState.Falling);
            return;
        }

        if (!InputManager.instance.isPlayerMoving)
        {
            m_Player.ChangeState(PlayerState.Idle);
            return;
        }

        if (InputManager.instance.isPlayerJumpPerformed)
        {
            m_Player.ChangeState(PlayerState.Jump);
            return;
        }

        if (!InputManager.instance.shouldPlayerRun)
        {
            m_Player.ChangeState(PlayerState.Walk);
            return;
        }              

        Move();
    }

    private void OnLeftFootDown()
    {
        m_FootStep = EFootStep.LeftFoot;
    }

    private void OnRightFootDown()
    {
        m_FootStep = EFootStep.RightFoot;
    }
}
