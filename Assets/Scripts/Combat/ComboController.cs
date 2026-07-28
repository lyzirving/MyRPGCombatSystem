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

    /// <summary>
    /// Find which combo sequence index starts with the given action type
    /// </summary>
    public int FindComboIndexByStartAction(CombatDefine.EAttack action)
    {
        if (m_ComboSequences == null) return -1;
        
        for (int i = 0; i < m_ComboSequences.Length; i++)
        {
            var combo = m_ComboSequences[i];
            if (combo != null && combo.skillConfigs != null && combo.skillConfigs.Length > 0 && 
                combo.skillConfigs[0].action == action)
                return i;
        }

        // return invalid index if no match found
        return -1; 
    }

    /// <summary>
    /// Try to advance the combo with the given input action.
    /// First checks if we can advance within the current combo sequence.
    /// If not, checks if we can switch to a different combo at the same skill position.
    /// Returns true if advancement was successful (either within combo or cross-combo switch).
    /// </summary>
    public bool TryAdvanceCombo(CombatDefine.EAttack inputAction)
    {
        if (!isComboStart)
            return false;

        // 1. Try to advance within current combo
        if (hasNextSkill && CanAdvanceNextSkill(inputAction))
        {
            NextSkill();
            return true;
        }

        // 2. Try to switch to a different combo at the same next skill position
        int nextSkillIdx = m_SkillIndex + 1;
        for (int i = 0; i < m_ComboSequences.Length; i++)
        {
            if (i == m_ComboIndex) continue;
            
            var combo = m_ComboSequences[i];
            if (combo == null || combo.skillConfigs == null || combo.skillConfigs.Length <= nextSkillIdx)
                continue;
            
            if (combo.skillConfigs[nextSkillIdx].action == inputAction)
            {
                // Switch to this combo branch at the same skill position
                m_ComboIndex = i;
                m_SkillIndex = nextSkillIdx;
                m_StartTime = -1f;
                return true;
            }
        }

        return false;
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
