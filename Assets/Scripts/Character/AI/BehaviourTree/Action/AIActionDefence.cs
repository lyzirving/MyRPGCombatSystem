using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class AIActionDefence : AIBehaviourAction
{
    public override TaskStatus OnUpdate()
    {
        Debug.LogWarning("AIActionDefence");
        return TaskStatus.Success;
    }
}
