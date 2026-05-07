using UnityEngine;

public class PlayerStateIdle : PlayerStateLocomotion
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {               
        base.Enter(exitState, args);

        bool immediateChange = (exitState != null) &&
                               (exitState.GetType() != typeof(PlayerStateMove)) &&
                               (exitState.GetType() != typeof(PlayerStateStrafeMove));
        if (immediateChange)
        {
            m_Player.model.SetAnimationFloat(AnimationConsts.speed, 0f);
            m_Player.model.SetAnimationFloat(AnimationConsts.angular, 0f);
        }
        m_Player.attrs.speedModify = 0f;
    }

    public override bool HandleInput()
    {
        if (m_Player.action.isMoving)
        {
            m_Player.ChangeState(m_Player.lockTarget != null ? ECharacterState.StrafeMove : ECharacterState.Move);
            return true;
        }
        return false;
    }

    public override void Update()
    {               
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

    public override bool CanExecute(ECharacterAction action)
    {
        switch (action)
        {
            case ECharacterAction.Dodge:
                return m_Player.lockTarget != null;
            default:
                return true;
        }
    }

    public override void Execute(ECharacterAction action)
    {
        switch (action)
        {
            case ECharacterAction.Defence:
                m_Player.ChangeState(ECharacterState.Defence);
                return;
            case ECharacterAction.Jump:
                m_Player.ChangeState(ECharacterState.Jump);
                return;
            case ECharacterAction.LightAttack:
                m_Player.ChangeState(ECharacterState.Attack);
                return;               
            case ECharacterAction.Dodge:
                m_Player.MakeDodgeAction(m_Player.action.playerMovement);
                m_Player.ChangeState(ECharacterState.Dodge);
                return;
            default:
                break;
        }
    }
}
