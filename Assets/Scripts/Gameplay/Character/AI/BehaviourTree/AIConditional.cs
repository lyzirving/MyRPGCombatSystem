using BehaviorDesigner.Runtime.Tasks;

public class AIConditional : Conditional
{
    protected AIController m_AIController;

    public override void OnAwake()
    {
        m_AIController = GetComponent<AIController>();
        if (m_AIController == null)
        {
            throw new System.Exception("AIController hasn't been assgined to gameobject width behaviour!");
        }
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}
