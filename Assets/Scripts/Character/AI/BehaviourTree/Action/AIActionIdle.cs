using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class AIActionIdle : AIBehaviourAction
{
    public override void OnStart()
    {
        m_AIController.ChangeState(ECharacterState.Idle);
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Running;
    }
}
