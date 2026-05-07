using System;
using UnityEngine;

public class ComboController
{
    private ICharacterBehavior m_PlayerBehavior = null;
    private ComboSequence[] m_ComboSequence = null;    
    private float m_StartTime = -1;

    // Index of current combo
    public int comboIndex = 0;
    // Index of skill in current combo
    public int skillIndex = 0;
    public bool isComboStart { get => m_StartTime > 0f; }
    public bool hasNextSkill { get => skillIndex < (m_ComboSequence[comboIndex].skillConfigs.Length - 1); }
    public SkillData currentSkill { get => m_ComboSequence[comboIndex].skillConfigs[skillIndex]; }
    public SkillData nextSkill { get => m_ComboSequence[comboIndex].skillConfigs[skillIndex + 1]; }

    public void Init(ICharacterBehavior playerBehavior, ComboSequence[] comboSequences)
    {
        m_PlayerBehavior = playerBehavior;
        m_ComboSequence = comboSequences;
    }

    public void DeInit()
    {
        m_PlayerBehavior = null;
        m_ComboSequence = null;
    }

    public void BeginCombo()
    {
        m_StartTime = Time.time;
    }

    public void EndCombo()
    {
        skillIndex = 0;
        m_StartTime = -1f;
    }

    public bool GoNextSkill()
    {
        if (!isComboStart || !hasNextSkill)
            return false;

        // Check whether time exceeds the input floating window
        if (Time.time > (m_StartTime + nextSkill.inputWindowDuration))
            return false;

        return true;
    }

    public void NextSkill()
    { 
        skillIndex++;
        m_StartTime = -1f;
    }
}
