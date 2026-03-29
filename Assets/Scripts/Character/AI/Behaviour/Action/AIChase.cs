using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class AIChase : AIBehaviourAction
{
    public SharedTransform target;

    public override void OnStart()
    {
        m_AIController.ChangeState(ECharacterState.Move);
    }

    public override void OnEnd()
    {
        m_AIController.ChangeState(ECharacterState.Idle);
    }

    public override TaskStatus OnUpdate()
    {
        if (target == null || target.Value == null) return TaskStatus.Failure;        
        return TaskStatus.Running;
    }

    public override void OnFixedUpdate()
    {
        //TODO: move by AI's configuration
        m_AIController.MoveToImmediately(target.Value, 2f, 8f);
    }
}
