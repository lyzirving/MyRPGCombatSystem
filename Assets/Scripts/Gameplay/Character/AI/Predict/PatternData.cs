using System.Collections.Generic;

public class PatternData
{
    private long m_PatternKey;
    // Subsequent behavior statistics
    private Dictionary<ECharacterAction, int> m_NextActions = new Dictionary<ECharacterAction, int>();
    private int m_TotalOccurrences = 0;
    private string m_Description;

    public long key
    {
        get => m_PatternKey;
        set => m_PatternKey = value;
    }

    public string description
    {
        get => m_Description;
        set => m_Description = value;
    }

    public void AddNextAction(ECharacterAction action)
    {
        if (!m_NextActions.ContainsKey(action))
            m_NextActions[action] = 0;

        m_NextActions[action]++;
        m_TotalOccurrences++;
    }

    public ECharacterAction GetMostLikelyAction()
    {
        ECharacterAction bestAction = ECharacterAction.None;
        int bestCount = 0;

        foreach (var kvp in m_NextActions)
        {
            if (kvp.Value > bestCount)
            {
                bestCount = kvp.Value;
                bestAction = kvp.Key;
            }
        }

        return bestAction;
    }

    public float GetConfidence(ECharacterAction action)
    {
        if (m_TotalOccurrences == 0 || !m_NextActions.ContainsKey(action))
            return 0f;

        return (float)m_NextActions[action] / m_TotalOccurrences;
    }
}
