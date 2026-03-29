
using UnityEngine;

public class AIStateMove : AIStateLocomotion
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        if (exitState != null && exitState.GetType() != typeof(AIStateMove))
        {
            m_AIController.model.SetAnimationFloat(AnimationConsts.speed, 0f);
            m_AIController.model.SetAnimationFloat(AnimationConsts.angular, 0f);
        }
        m_AIController.attrs.speedModify = 0.7f;
    }

    public override void Update()
    {
        m_AIController.model.SetAnimationFloat(AnimationConsts.speed, 1f, 0.1f, Time.deltaTime);
    }
}
