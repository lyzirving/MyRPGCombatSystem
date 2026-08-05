using UnityEngine;

public class PlayerStateStrafeMove : PlayerStateMove
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {    
        base.Enter(exitState, args);
        m_Player.model.SetAnimationBool(AnimationConsts.strafe, true);   
    }

    public override bool Exit(StateBase newState)
    {      
        m_Player.model.SetAnimationBool(AnimationConsts.strafe, false);
        return base.Exit(newState);
    }

    public override void FixedUpdate()
    {
        if (!m_Player.action.isMoving || m_Player.lockTarget == null)
            return;

        m_Player.attrs.speedModify = m_Player.config.move.runModify;
        m_Player.RotateToTargetDir(m_Player.action.cameraFwd.NormalizeIgnoreY(), m_Player.config.move.rotateSpeed);        

        Vector2 input = m_Player.action.playerMovement;
        Vector3 moveDir = m_Player.transform.right * input.x + m_Player.transform.forward * input.y;
        moveDir.y = 0;
        moveDir.Normalize();
        MoveImmediately(moveDir * m_Player.speedScaler);
    }

    protected override void UpdateAnimationValue()
    {
        Vector2 input = m_Player.action.playerMovement;
        m_Player.model.SetAnimationFloat(AnimationConsts.speed, input.y, 0.1f, Time.deltaTime);
        m_Player.model.SetAnimationFloat(AnimationConsts.angular, input.x, 0.1f, Time.deltaTime);
    }
}
