using UnityEngine;

public class PlayerStateMove : PlayerStateLocomotion
{
    public override void Update()
    {
        if (m_Player.action.isLightAttack)
        {
            m_Player.ChangeState(ECharacterState.Attack);
            return;
        }        

        if (m_Player.action.isJump)
        {
            m_Player.ChangeState(ECharacterState.Jump);
            return;
        }

        if (!m_Player.action.isMoving)
        {
            m_Player.ChangeState(ECharacterState.Idle);
            return;
        }

        float speed = m_Player.action.shouldRun ? 2f : 1f;
        m_Player.model.SetAnimationFloat(AnimationConsts.speed, speed, 0.1f, Time.deltaTime);

        Vector3 forward = m_Player.transform.forward;
        Vector3 targetDir = m_Player.GetTargetDirection();        
        float angle = Mathf.Rad2Deg * Mathf.Acos(Mathf.Clamp(Vector3.Dot(forward, targetDir), -1f, 1f));        
        float angular = Mathf.Clamp(angle / 60f, 0f, 1f);
        float sign = Mathf.Sign(Vector3.Cross(forward, targetDir).y);
        m_Player.model.SetAnimationFloat(AnimationConsts.angular, angular * sign, 0.1f, Time.deltaTime);
        Debug.Log($"angle[{angle}], sign[{sign}], angular[{angular}]");        
        //float value = m_Player.model.GetAnimationFloat(AnimationConsts.angular);
        //value += sign * radians;
        //value = Mathf.Clamp(value, -1f, 1f);        
        //Debug.Log($"radians[{radians}], sign[{sign}], value[{value}]");
    }

    public override void FixedUpdate()
    {
        if (!m_Player.action.isMoving)
            return;

        m_Player.attrs.speedModify = m_Player.action.shouldRun ? m_Player.config.runSpeedModify : m_Player.config.walkSpeedModify;
        Vector3 targetDir = m_Player.GetTargetDirection();
        m_Player.RotateToTargetDir(targetDir, m_Player.config.rotateSpeed);
        Move(targetDir * m_Player.movementSpeed);
    }    
}
