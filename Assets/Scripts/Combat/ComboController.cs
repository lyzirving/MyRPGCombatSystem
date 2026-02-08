using System;
using UnityEngine;

public class ComboController
{
    private IPlayerBehavior m_PlayerBehavior = null;
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

    public void Init(IPlayerBehavior playerBehavior, ComboSequence[] comboSequences)
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

    /// <summary>
    /// Update combo status
    /// </summary>
    /// <returns>true, if we should switch to next skill</returns>
    public bool UpdateCombo()
    {
        if (!isComboStart || !hasNextSkill)
            return false;

        if(Time.time > (m_StartTime + nextSkill.inputWindowDuration))
            return false;

        if (IsSkillActionPeform(nextSkill.action))
            return true;

        return false;
    }

    public void NextSkill()
    { 
        skillIndex++;
        m_StartTime = -1f;
    }

    private bool IsSkillActionPeform(CombatDefine.EAttack action)
    {
        switch (action)
        {
            case CombatDefine.EAttack.LP:
                return m_PlayerBehavior.PlayerAction().isLightPunch;
            case CombatDefine.EAttack.LK:
            default:
                return false;
        }
    }
}
