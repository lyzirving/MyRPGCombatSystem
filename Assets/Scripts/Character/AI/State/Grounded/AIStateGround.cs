using UnityEngine;

public class AIStateGround : AIStateBase
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_AIController.model.StartAnimation(AnimationConsts.ground);
    }

    public override void Exit(StateBase newState)
    {
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(AIStateGround)))
        {
            m_AIController.model.StopAnimation(AnimationConsts.ground);
        }
    }

    public override void FixedUpdate()
    {
        m_AIController.ResetVelocity();
    }

    public override void OnExitGround()
    {
        m_AIController.ChangeState(ECharacterState.Falling);
    }
}
