using UnityEngine;

public class AIStateGround : AIStateBase
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        
    }

    public override void Exit(StateBase newState)
    {
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(AIStateGround)))
        {
            
        }
    }

    protected override void OnExitGround(Collider collider)
    {
    }
}
