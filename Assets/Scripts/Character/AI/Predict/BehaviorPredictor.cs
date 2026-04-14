using BehaviorDesigner.Runtime;
using UnityEngine;

[System.Serializable]
public class PredictionSettings
{
    public int patternLength = 3;
    public float predictionConfidence = 0.7f;
    public int maxHistorySize = 100;
}

public class BehaviorPredictor : MonoBehaviour
{
    [SerializeField] private float m_DetectActionInterval = 0.15f;
    [SerializeField] private PatternAnalyzer m_PatternAnalyzer = new PatternAnalyzer();    
    [SerializeField] private CharacterControllerBase m_TargetCharacter;

    private DistanceZone m_DistanceZone;
    private float m_Confidence = 0f;
    private float m_LastConfidence = 0f;
    private ECharacterAction m_Response = ECharacterAction.None;
    private ECharacterAction m_LastResponse = ECharacterAction.None;

    private ECharacterAction m_LastDetectedAction = ECharacterAction.None;
    private ECharacterAction m_LastPredictedAction = ECharacterAction.None;    
    private float m_LastDetectActionTime;

    private float m_ExecuteTime;
    private float m_Interval = 0.1f;

    private SharedInt m_BehaviorResponse;
    private SharedFloat m_BehaviorConfidence;

    private void Awake()
    {
        m_PatternAnalyzer.Init();
        m_LastDetectActionTime = Time.time;        
    }

    private void Start()
    {
        m_DistanceZone = GetComponent<DistanceZone>();
        m_DistanceZone.target = m_TargetCharacter.transform;

        m_ExecuteTime = 0f;

        BehaviorTree bt = GetComponent<BehaviorTree>();
        m_BehaviorResponse = bt.GetVariable(AIConsts.STR_RESPONSE) as SharedInt;
        m_BehaviorConfidence = bt.GetVariable(AIConsts.STR_CONFIDENCE) as SharedFloat;
    }

    private void Update()
    {
        if (Mathf.Approximately(m_ExecuteTime, 0f) || (Time.time - m_ExecuteTime > m_Interval))
        {
            Execute();

            m_ExecuteTime = Time.time;

            m_BehaviorResponse.SetValue((int)m_Response);
            m_BehaviorConfidence.SetValue(m_Confidence);
        }        
    }

    private void Execute()
    {
        long currentPattern = 0;
        float currentConfidence = 0;        
        ECharacterAction predictedAction = m_LastPredictedAction;
        ECharacterAction response = m_LastResponse;

        ECharacterAction currentDetectedAction = DetectPlayerAction();
        if (m_PatternAnalyzer.Predict())
        {            
            currentConfidence = m_PatternAnalyzer.confidence;
            currentPattern = m_PatternAnalyzer.pattern;
            predictedAction = m_PatternAnalyzer.prediction;
            if (currentConfidence <= m_PatternAnalyzer.settings.predictionConfidence)
            {
                currentConfidence = m_LastConfidence;
                response = m_LastResponse;
            }
            else
            {
                response = MakeActionWithPrecition(predictedAction, currentConfidence);
            }
        }
        else
        {
            currentConfidence = m_LastConfidence;
            response = m_LastResponse;
        }

        m_Confidence = currentConfidence;
        m_Response = response;

        // For next prediction
        m_PatternAnalyzer.RecordAction(currentDetectedAction);
        m_PatternAnalyzer.UpdatePatternDatabase(currentPattern, currentDetectedAction);

        m_LastDetectedAction = currentDetectedAction;
        m_LastPredictedAction = predictedAction;
        m_LastResponse = m_Response;
        m_LastConfidence = m_Confidence;
    }

    private ECharacterAction DetectPlayerAction()
    {
        if (m_TargetCharacter == null)
        {
            Debug.LogWarning("Warning! target character hasn't been assigned yet!");
            return ECharacterAction.None;
        }

        if (m_TargetCharacter.IsInAnimationTransition() && m_LastDetectedAction != ECharacterAction.None)
            return m_LastDetectedAction;

        if (Time.time - m_LastDetectActionTime < m_DetectActionInterval && m_LastDetectedAction != ECharacterAction.None)
            return m_LastDetectedAction;

        m_LastDetectActionTime = Time.time;        

        return m_TargetCharacter.GetCurrentAction();
    }

    private ECharacterAction MakeActionWithPrecition(ECharacterAction nextAction, float confidence)
    {
        ECharacterAction response = ECharacterAction.None;
        switch (m_DistanceZone.zone)
        {
            case EDistanceZone.CloseCombatRange:
                if (nextAction == ECharacterAction.Attack)
                    response = ECharacterAction.Defence;
                else
                    response = ECharacterAction.Attack;
                break;
            default:
                break;
        }
        return response;
    }
}
