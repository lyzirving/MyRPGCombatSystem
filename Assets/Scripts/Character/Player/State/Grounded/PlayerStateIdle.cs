using UnityEngine;

public class PlayerStateIdle : PlayerStateLocomotion
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {               
        if(exitState != null && exitState.GetType() != typeof(PlayerStateMove))
        {
            m_Player.model.SetAnimationFloat(AnimationConsts.speed, 0f);
            m_Player.model.SetAnimationFloat(AnimationConsts.angular, 0f);
        }
        m_Player.attrs.speedModify = 0f;
    }

    public override void Update()
    {
        bool isComingIn = m_Player.model.animator.IsTransitToState("Locomotion", AnimationConsts.BASE_LAYER);        
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

        if (m_Player.action.holdDefence)
        {
            m_Player.ChangeState(ECharacterState.Defence);
            return;
        }

        if (m_Player.action.isMoving)
        {
            m_Player.ChangeState(ECharacterState.Move);
            return;
        }

        m_Player.model.SetAnimationFloat(AnimationConsts.speed, 0f, 0.1f, Time.deltaTime);
        m_Player.model.SetAnimationFloat(AnimationConsts.angular, 0f, 0.1f, Time.deltaTime);
    }

    public override void FixedUpdate()
    {
        m_Player.ResetHorizontalVelocity();
    }

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Idle;
    }
}
