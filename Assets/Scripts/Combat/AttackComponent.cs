using UnityEngine;

public class AttackComponent : MonoBehaviour
{
    [SerializeField] private ComboSequence[] m_ComboSequences;
    
    private AttackHotspot[] m_Hotspots;
    private int m_ComboIndex = 0;
    private int m_SkillIndex = 0;

    private ComboSystem m_ComboSystem = new();

    public ComboSequence combo { get => m_ComboSequences[m_ComboIndex]; }
    public SkillData skill { get => combo.skillConfigs[m_SkillIndex]; }
    public AttackHotspot hotspot { get => m_Hotspots[skill.hotspotIndex]; }

    #region Main Method
    public void Init(IPlayerBehavior playerBehavior)
    {        
        m_Hotspots = GetComponentsInChildren<AttackHotspot>();
        int len = m_Hotspots == null ? 0 : m_Hotspots.Length;
        if (len == 0)
            throw new System.Exception("No attack hotspots is bound");

        for (int i = 0; i < m_Hotspots.Length; ++i)
        {
            m_Hotspots[i].Init(playerBehavior);
            BindComboSkillData(i, m_Hotspots[i], m_ComboSequences);
        }
    }
    #endregion

    #region Main Method
    public void StartCombo()
    {
        m_ComboSystem.BeginCombo(combo, m_SkillIndex);
    }

    public bool UpdateCombo()
    {
        return m_ComboSystem.UpdateCombo();
    }

    public void ResetCombo()
    {
        m_ComboSystem.EndCombo();
        m_ComboIndex = 0;
        m_SkillIndex = 0;
    }
    #endregion

    private void BindComboSkillData(int index, AttackHotspot hotspot, ComboSequence[] combos)
    { 
        if(combos == null || combos.Length == 0 || hotspot == null || index < 0)
            return;

        for (int i = 0; i < combos.Length; ++i)
        {
            SkillData[] skills = combos[i].skillConfigs;
            for (int j = 0; j < skills.Length; ++j)
            {
                if (skills[j].attackHotspot == hotspot.name)
                    skills[j].hotspotIndex = index;
            }
        }
    }
}
