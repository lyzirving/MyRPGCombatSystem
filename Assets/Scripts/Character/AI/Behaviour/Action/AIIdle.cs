using BehaviorDesigner.Runtime.Tasks;

public class AIIdle : AIBehaviourAction
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
