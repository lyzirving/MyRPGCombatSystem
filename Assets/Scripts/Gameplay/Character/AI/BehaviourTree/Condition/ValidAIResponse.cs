using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class ValidAIResponse : AIConditional
{
    public ECharacterAction target = ECharacterAction.None;
    public float confidence = 0.7f;

    private SharedInt m_Response;
    private SharedFloat m_Confidence;

    public override void OnAwake()
    {
        base.OnAwake();

        m_Response = Owner.GetVariable(AIConsts.STR_RESPONSE) as SharedInt;
        m_Confidence = Owner.GetVariable(AIConsts.STR_CONFIDENCE) as SharedFloat;
    }

    public override TaskStatus OnUpdate()
    {
        return ((m_Response.Value == (int)target) && (m_Confidence.Value >= confidence)) ? 
            TaskStatus.Success : TaskStatus.Failure;
    }
}
