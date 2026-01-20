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
        if (!InputManager.instance.isPlayerMoving)
        {
            m_Player.ChangeState(EPlayerState.Idle);
            return;
        }

        if (InputManager.instance.isPlayerRollPerformed)
        {
            m_Player.ChangeState(EPlayerState.Roll);
            return;
        }

        if (InputManager.instance.isPlayerJumpPerformed)
        {
            ChangeStateArgs.Builder builder = new ChangeStateArgs.Builder();
            builder.Footstep(m_FootStep);
            m_Player.ChangeState(EPlayerState.Jump, builder.Build());
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
        if (!InputManager.instance.isPlayerMoving)
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
