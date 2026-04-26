using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class AIActionChase : AIBehaviourAction
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
        m_AIController.MoveToImmediately(target.Value, m_AIController.runSpeedScaler, m_AIController.config.move.rotateSpeed);
    }
}
