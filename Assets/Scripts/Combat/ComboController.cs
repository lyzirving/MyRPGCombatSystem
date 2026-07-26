using UnityEngine;

public class ComboController
{
    private ComboSequence[] m_ComboSequences = null; 

    /// <summary>
    /// Time when the combo starts
    /// </summary>   
    private float m_StartTime = -1;

    /// <summary>
    /// Index of current combo
    /// </summary>
    private int m_ComboIndex = 0;

    /// <summary>
    /// Index of skill in current combo
    /// </summary>
    private int m_SkillIndex = 0;

    public bool hasSkill => m_ComboSequences != null && m_ComboSequences.Length > 0;
    public bool isComboStart { get => m_StartTime > 0f; }
    public bool hasNextSkill { get => m_SkillIndex < (m_ComboSequences[m_ComboIndex].skillConfigs.Length - 1); }

    public ComboSequence combo { get => m_ComboSequences[m_ComboIndex]; }
    public SkillData skill { get => m_ComboSequences[m_ComboIndex].skillConfigs[m_SkillIndex]; }    
    public SkillData currentSkill { get => m_ComboSequences[m_ComboIndex].skillConfigs[m_SkillIndex]; }
    public SkillData nextSkill { get => m_ComboSequences[m_ComboIndex].skillConfigs[m_SkillIndex + 1]; }

    public void Init(ComboSequence[] comboSequences)
    {
        m_ComboSequences = comboSequences;
    }

    public void DeInit()
    {
        m_ComboSequences = null;
    }

    public void SetComboIndex(int index)
    {
        m_ComboIndex = index;
        m_SkillIndex = 0;
        m_StartTime = -1f;
    }

    public void BeginCombo()
    {
        m_StartTime = Time.time;
    }

    public void EndCombo()
    {
        m_SkillIndex = 0;
        m_StartTime = -1f;
    }

    public bool CanAdvanceNextSkill(CombatDefine.EAttack inputAction)
    {
        if (!isComboStart || !hasNextSkill)
            return false;

        // Check whether time exceeds the input floating window
        if (Time.time > (m_StartTime + nextSkill.inputWindowDuration))
            return false;

        return nextSkill.action == inputAction;
    }

    public void NextSkill()
    { 
        m_SkillIndex++;
        m_StartTime = -1f;
    }
}
