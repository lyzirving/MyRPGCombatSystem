using UnityEngine;

public class AIStateCombat : AIStateGround
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_AIController.model.StartAnimation(AnimationConsts.combat);
    }

    public override bool Exit(StateBase newState)
    {
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(AIStateCombat)))
        {
            m_AIController.model.StopAnimation(AnimationConsts.combat);
        }
        return true;
    }
}
