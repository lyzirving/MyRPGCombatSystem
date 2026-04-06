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
    [SerializeField] private DistanceZone m_DistanceZone = new DistanceZone();
    [SerializeField] private CharacterControllerBase m_TargetCharacter;

    private float m_Confidence = 0f;
    private ECharacterAction m_Response = ECharacterAction.None;
    private ECharacterAction m_LastResponse = ECharacterAction.None;

    private ECharacterAction m_LastDetectedAction = ECharacterAction.None;
    private ECharacterAction m_LastPredictedAction = ECharacterAction.None;    
    private float m_LastDetectActionTime;

    public ECharacterAction response => m_Response;
    public float confidence => m_Confidence;

    private void Awake()
    {
        m_PatternAnalyzer.Init();
        m_LastDetectActionTime = Time.time;        
    }

    private void Start()
    {
        m_DistanceZone.source = this.transform;
        m_DistanceZone.target = m_TargetCharacter.transform;
    }

    public void Execute()
    {
        long currentPattern = 0;
        float currentConfidence = 0;        
        ECharacterAction predictedAction = m_LastPredictedAction;

        m_DistanceZone.Update();

        ECharacterAction currentAction = DetectPlayerAction();
        if (m_PatternAnalyzer.Predict())
        {
            currentConfidence = m_PatternAnalyzer.confidence;
            currentPattern = m_PatternAnalyzer.pattern;
            predictedAction = m_PatternAnalyzer.prediction;
            m_Response = MakeActionWithPrecition(predictedAction, currentConfidence);
        }
        else
        {
            m_Response = MakeAction();
        }
        m_Confidence = currentConfidence;

        // For next prediction
        m_PatternAnalyzer.RecordAction(currentAction);
        m_PatternAnalyzer.UpdatePatternDatabase(currentPattern, currentAction);

        m_LastDetectedAction = currentAction;
        m_LastPredictedAction = predictedAction;
        m_LastResponse = m_Response;
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

    private ECharacterAction MakeAction()
    {
        return ECharacterAction.None;
    }

    private ECharacterAction MakeActionWithPrecition(ECharacterAction nextAction, float confidence)
    {
        if (confidence <= m_PatternAnalyzer.settings.predictionConfidence)
            return MakeAction();

        if (m_DistanceZone.zone == EDistanceZone.CloseCombatRange)
        {
            if (nextAction == ECharacterAction.Attack)
            {
                return ECharacterAction.Defence;
            }
        }
        else
        {
            return ECharacterAction.Idle;
        }

        return ECharacterAction.None;
    }
}
