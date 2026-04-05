using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PatternAnalyzer
{
    [SerializeField] private PredictionSettings m_Settings = new PredictionSettings();

    private List<ECharacterAction> m_ActionHistory = new List<ECharacterAction>();
    private Dictionary<long, PatternData> m_PatternDatabase = new Dictionary<long, PatternData>();
    private PatternEncoder m_Encoder;

    private long m_Pattern = 0;
    private float m_Confidence = 0f;
    private ECharacterAction m_Prediction = ECharacterAction.None;

    public long pattern => m_Pattern;
    public float confidence => m_Confidence;
    public ECharacterAction prediction => m_Prediction;
    public PredictionSettings settings => m_Settings;

    public void Init()
    {
        m_Encoder = new PatternEncoder((int)ECharacterAction.Count, m_Settings.patternLength);
    }

    public bool Predict()
    {
        if (!CanPredict())
        {
            m_Prediction = ECharacterAction.None;
            m_Confidence = 0f;
            m_Pattern = 0;
            return false;
        }

        m_Pattern = GetCurrentPattern();
        m_Prediction = PredictNextAction(m_Pattern, out m_Confidence);
        return true;
    }

    public void RecordAction(ECharacterAction action)
    {
        if (action == ECharacterAction.None) return;

        m_ActionHistory.Add(action);

        if (m_ActionHistory.Count > m_Settings.maxHistorySize)
        {
            m_ActionHistory.RemoveAt(0);
        }
    }

    public void UpdatePatternDatabase(long patternKey, ECharacterAction nextAction)
    {
        if (patternKey == 0)
            return;

        if (!m_PatternDatabase.ContainsKey(patternKey))
        {
            var data = new PatternData();
            data.key = patternKey;
            data.description = m_Encoder.MakeDescription(patternKey);
            m_PatternDatabase.Add(patternKey, data);
        }

        m_PatternDatabase[patternKey].AddNextAction(nextAction);
    }

    private bool CanPredict()
    {
        return m_ActionHistory.Count >= m_Settings.patternLength;
    }

    private long GetCurrentPattern()
    {
        // Get latest action with patternLength
        int[] recentActions = new int[m_Encoder.patternLength];
        for (int i = m_ActionHistory.Count - m_Settings.patternLength, j = 0; i < m_ActionHistory.Count; ++i, ++j)
            recentActions[j] = (int)m_ActionHistory[i];

        return m_Encoder.FastEncode(recentActions);
    }

    private ECharacterAction PredictNextAction(long currentPattern, out float confidence)
    {
        confidence = 0f;

        if (currentPattern == 0)
            return ECharacterAction.None;

        if (m_PatternDatabase.ContainsKey(currentPattern))
        {
            PatternData pattern = m_PatternDatabase[currentPattern];
            ECharacterAction predicted = pattern.GetMostLikelyAction();
            confidence = pattern.GetConfidence(predicted);
            //Debug.Log($"predict action[{predicted}], key[{pattern.key}], pattern[{pattern.description}], confidence[{confidence}]");
            return predicted;
        }

        return ECharacterAction.None;
    }
}
