using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class AIActionPredict : AIBehaviourAction
{
    private BehaviorPredictor m_Predictor;

    private SharedInt m_Response;
    private SharedFloat m_Confidence;

    private float m_PredictTime;
    private float m_Interval = 0.1f;
    private bool m_IsFirst = true;

    public override void OnAwake()
    {
        base.OnAwake();
        m_Predictor = GetComponent<BehaviorPredictor>();

        m_Response = Owner.GetVariable(AIConsts.STR_RESPONSE) as SharedInt;
        m_Confidence = Owner.GetVariable(AIConsts.STR_CONFIDENCE) as SharedFloat;

        m_IsFirst = true;
        m_PredictTime = Time.time;
    }

    public override TaskStatus OnUpdate()
    {
        if(m_Predictor == null)
            return TaskStatus.Failure;

        if (m_IsFirst || (Time.time - m_PredictTime > m_Interval))
        {
            m_Predictor.Execute();
            m_Response.SetValue((int)m_Predictor.response);
            m_Confidence.SetValue(m_Predictor.confidence);

            m_IsFirst = false;
            m_PredictTime = Time.time;
            return TaskStatus.Success;
        }
        else
        {
            return TaskStatus.Running;
        }
    }
}
