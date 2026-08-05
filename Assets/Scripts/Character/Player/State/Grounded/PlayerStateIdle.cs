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
}
