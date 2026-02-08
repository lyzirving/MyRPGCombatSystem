using UnityEngine;

public class PlayerStateMove : PlayerStateGrounded
{
    protected EFootStep m_FootStep = EFootStep.LeftFootStep;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.RegisterLeftFootStepAction(OnLeftFootStep);
        m_Player.model.RegisterRightFootStepAction(OnRightFootStep);
        m_Player.model.StartAnimation(m_Player.animConsts.moveHash);
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.RemoveLeftFootStepAction(OnLeftFootStep);
        m_Player.model.RemoveRightFootStepAction(OnRightFootStep);
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(PlayerStateMove)))
        {
            m_Player.model.StopAnimation(m_Player.animConsts.moveHash);
        }
        base.Exit(newState);
    }

    public override void Update()
    {
        if (m_Player.action.isLightPunch)
        {
            m_Player.ChangeState(EPlayerState.StandardAttack);
            return;
        }

        if (!m_Player.action.isMoving)
        {
            m_Player.ChangeState(EPlayerState.Idle);
            return;
        }

        if (m_Player.action.isRoll)
        {
            m_Player.ChangeState(EPlayerState.Roll);
            return;
        }

        if (m_Player.action.isJump)
        {
            m_Player.ChangeState(EPlayerState.Jump, new ChangeStateArgs.Builder(m_FootStep).Build());
            return;
        }
    }

    public override void FixedUpdate()
    {
        Float();

        Move();
    }    

    protected void Move()
    {
        if (!m_Player.action.isMoving)
            return;

        Vector3 targetDir = GetTargetDirection();

        RotateToTargetDir(targetDir);

        MoveAt(targetDir);
    }

    protected void MoveAt(in Vector3 targetDir)
    {
        m_Player.rigidBody.AddForce(targetDir * movementSpeed - playerHorizonVelocity, ForceMode.VelocityChange);
    }

    protected void OnLeftFootStep()
    {
        m_FootStep = EFootStep.LeftFootStep;
    }

    protected void OnRightFootStep()
    {
        m_FootStep = EFootStep.RightFootStep;
    }
}
