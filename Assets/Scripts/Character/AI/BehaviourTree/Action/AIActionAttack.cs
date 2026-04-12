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
            bool isState = m_AIController.currentState is AIStateAttack;
            if (!isState)
            {
                m_StartTime = Time.time;
                m_AIController.ChangeState(ECharacterState.Attack);                
            }
        }
        return TaskStatus.Running;
    }
}
