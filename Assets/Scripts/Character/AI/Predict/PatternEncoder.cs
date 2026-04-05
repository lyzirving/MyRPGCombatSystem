using System;
using System.Text;

public class PatternEncoder
{
    private int m_BaseValue;     
    private int m_PatternLength; 

    public int patternLength => m_PatternLength;

    public PatternEncoder(int behaviorTypesCount, int patternLen)
    {
        m_BaseValue = behaviorTypesCount;
        m_PatternLength = patternLen;
    }

    public long FastEncode(int[] behaviorSequence)
    {
        if (behaviorSequence.Length != m_PatternLength)
            throw new ArgumentException($"sequence length must equal {m_PatternLength}");

        long key = 0;
        long multiplier = 1;

        for (int i = 0; i < m_PatternLength; i++)
        {
            if (behaviorSequence[i] < 0 || behaviorSequence[i] >= m_BaseValue)
                throw new ArgumentException($"behavior value is out of range [0, {m_BaseValue - 1}]");

            key += behaviorSequence[i] * multiplier;
            multiplier *= m_BaseValue;
        }

        return key;
    }

    public int[] Decode(long key)
    {
        int[] behaviorSequence = new int[m_PatternLength];
        long temp = key;

        for (int i = 0; i < m_PatternLength; i++)
        {
            behaviorSequence[i] = (int)(temp % m_BaseValue);
            temp /= m_BaseValue;
        }

        return behaviorSequence;
    }

    public string MakeDescription(long key)
    {
        string desc = null;
        if (key != 0)
        {
            int[] sequence = Decode(key);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < sequence.Length; i++)
            {
                ECharacterAction action = (ECharacterAction)sequence[i];
                sb.Append(action);
                if(i != sequence.Length - 1)
                    sb.Append("-");
            }
            desc = sb.ToString();
        }
        return desc;
    }
}
