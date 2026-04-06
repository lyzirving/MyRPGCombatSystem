using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class AIActionRoar : AIBehaviourAction
{
    public SharedTransform player;

    public override void OnStart()
    {
        m_AIController.ChangeState(ECharacterState.Roar);
        var state = m_AIController.currentState as AIStateRoar;
        state.target = player.Value;
    }

    public override TaskStatus OnUpdate()
    {
        return m_AIController.currentState is AIStateRoar ? TaskStatus.Running : TaskStatus.Success;
    }
}
