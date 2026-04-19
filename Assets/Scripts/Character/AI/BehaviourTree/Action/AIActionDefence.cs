using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class AIActionDefence : AIBehaviourAction
{
    public override void OnStart()
    {
        m_AIController.ChangeState(ECharacterState.Defence);
    }

    public override void OnEnd()
    {
        var state = m_AIController.GetCurrentState<AIStateDefence>();
        state?.ReleaseDefence();
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Running;
    }
}
