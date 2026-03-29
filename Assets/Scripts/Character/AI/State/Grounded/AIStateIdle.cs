
using UnityEngine;

public class AIStateIdle : AIStateLocomotion
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_AIController.attrs.speedModify = 0f;
    }

    public override void Update()
    {
        m_AIController.model.SetAnimationFloat(AnimationConsts.speed, 0f, 0.1f, Time.deltaTime);
        m_AIController.model.SetAnimationFloat(AnimationConsts.angular, 0f, 0.1f, Time.deltaTime);
    }

    public override void FixedUpdate()
    {
        m_AIController.ResetHorizontalVelocity();
    }
}
