using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

public class AIActionAttack : AIBehaviourAction
{
    public float interval = 0.15f;
    private float m_StartTime = 0;

    public override void OnStart()
    {
        m_StartTime = Time.time;
        m_AIController.ChangeState(ECharacterState.Attack);
    }

    public override void OnEnd()
    {
        m_AIController.ChangeState(ECharacterState.Idle);
    }

    public override TaskStatus OnUpdate()
    {
        if (Time.time - m_StartTime >= interval)
        {
            if (!m_AIController.IsCurrentState<AIStateAttack>())
            {
                m_StartTime = Time.time;
                m_AIController.ChangeState(ECharacterState.Attack);                
            }
        }
        return TaskStatus.Running;
    }
}
