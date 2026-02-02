using UnityEngine;

public class PlayerAttackComponent : MonoBehaviour
{
    [SerializeField] private ComboSequence[] m_ComboSequences;

    private AttackHotspot[] m_Hotspots;
    private int m_ComboIndex = 0;
    private int m_SkillIndex = 0;

    public ComboSequence combo { get => m_ComboSequences[m_ComboIndex]; }
    public SkillData skill { get => combo.skillConfigs[m_SkillIndex]; }
    public AttackHotspot hotspot { get => m_Hotspots[skill.hotspotIndex]; }

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
