using System.Collections.Generic;
using System;
using UnityEngine;

public enum EGameplayAttributeType
{ 
    None = 0,
    Health,
    Count
}

[Serializable]
public class GameplayAttribute
{
    private EGameplayAttributeType m_Type;
    private float m_BaseValue;
    private float m_MaxValue;
    private float m_CurrentValue;        
    private List<AttributeModifier> m_Modifiers = new List<AttributeModifier>();
    private bool m_IsDirty = true;

    public EGameplayAttributeType type => m_Type;
    public float baseValue
    {
        get => m_BaseValue;
        set
        {
            if (Mathf.Abs(m_BaseValue - value) > Mathf.Epsilon)
            {
                m_BaseValue = value < m_MaxValue ? value : m_MaxValue;
                m_IsDirty = true;
            }
        }
    }
    public float currentValue
    {
        get
        {
            if (m_IsDirty) 
                RecalculateValue();

            return m_CurrentValue;
        }
    }

    public GameplayAttribute(EGameplayAttributeType type, float baseValue = 0, float maxValue = float.MaxValue)
    {
        m_Type = type;
        m_BaseValue = baseValue < maxValue ? baseValue : maxValue;
        m_MaxValue = maxValue;
        m_CurrentValue = m_BaseValue;        
    }

    public void AddModifier(AttributeModifier modifier)
    {
        if (modifier == null) return;

        m_Modifiers.Add(modifier);
        AttributeModifier.Sort(m_Modifiers);
        m_IsDirty = true;
    }

    public bool RemoveModifier(AttributeModifier modifier)
    {
        bool removed = m_Modifiers.Remove(modifier);

        // if removed is false, it shouldn't modify dirty status
        if (removed) 
            m_IsDirty = true;

        return removed;
    }

    public void RemoveAllModifiers()
    {
        if (m_Modifiers.Count > 0)
        {
            m_Modifiers.Clear();
            m_IsDirty = true;
        }        
    }

    public void RemoveAllModifiersFromSource(object source)
    {
        int removed = m_Modifiers.RemoveAll(m => m.source == source);

        // if removed is false, it shouldn't modify dirty status
        if (removed > 0) 
            m_IsDirty = true;
    }

    public void ClearAllModifiers()
    {
        m_Modifiers.Clear();
        m_CurrentValue = m_BaseValue;
        m_IsDirty = true;
    }

    public override string ToString()
    {
        return m_Type.ToString();
    }

    private void RecalculateValue()
    {
        m_IsDirty = false;

        float finalValue = m_BaseValue;
        float addValue = 0;
        float multiplyValue = 1;

        foreach (var modifier in m_Modifiers)
        {
            switch (modifier.modifierType)
            {
                case ModifierType.Add:
                    addValue += modifier.value;
                    break;
                case ModifierType.Multiply:
                    // When multiply, modifier value always means percentage.
                    multiplyValue *= (1f + modifier.value);
                    break;
                case ModifierType.Override:
                    finalValue = modifier.value;
                    addValue = 0;
                    multiplyValue = 1;
                    break;
                default:
                    break;
            }
        }

        m_CurrentValue = (finalValue + addValue) * multiplyValue;
        m_CurrentValue = m_CurrentValue < m_MaxValue ? m_CurrentValue : m_MaxValue;
    }
}
