using UnityEngine;

public class PlayerStateMove : PlayerStateGrounded
{
    protected EFootStep m_FootStep = EFootStep.LeftFootStep;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.RegisterLeftFootStepAction(OnLeftFootStep);
        m_Player.model.RegisterRightFootStepAction(OnRightFootStep);
        m_Player.model.StartAnimation(AnimationConsts.move);
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.RemoveLeftFootStepAction(OnLeftFootStep);
        m_Player.model.RemoveRightFootStepAction(OnRightFootStep);
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(PlayerStateMove)))
        {
            m_Player.model.StopAnimation(AnimationConsts.move);
        }
        base.Exit(newState);
    }

    public override void Update()
    {
        if (m_Player.action.isLightAttack)
        {
            m_Player.ChangeState(ECharacterState.Attack);
            return;
        }

        if (!m_Player.action.isMoving)
        {
            m_Player.ChangeState(ECharacterState.Idle);
            return;
        }

        if (m_Player.action.isRoll)
        {
            m_Player.ChangeState(ECharacterState.Roll);
            return;
        }

        if (m_Player.action.isJump)
        {
            m_Player.ChangeState(ECharacterState.Jump);
            return;
        }
    }

    public override void FixedUpdate()
    {
        if (!m_Player.action.isMoving)
            return;

        Vector3 targetDir = m_Player.GetTargetDirection();

        m_Player.RotateToTargetDir(targetDir, m_Player.config.rotateSpeed);

        Move(targetDir * m_Player.movementSpeed);
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
