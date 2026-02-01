using UnityEngine;

public class PlayerAttackComponent : MonoBehaviour
{
    public ComboSequence[] comboSequences;

    private AttackHotspot[] m_Hotspots;
    private int m_Index = -1;

    public AttackHotspot[] hotspots { get { return m_Hotspots; } }
    public int index
    {
        get { return m_Index; }
        set { m_Index = value; }
    }
    public AttackHotspot currentHotspot { get => m_Hotspots[m_Index]; }

    public void Init(IPlayerBehavior playerBehavior)
    {
        m_Hotspots = GetComponentsInChildren<AttackHotspot>();
        int len = m_Hotspots == null ? 0 : m_Hotspots.Length;
        if (len == 0)
            throw new System.Exception("No attack hotspots is bound");

        for (int i = 0; i < m_Hotspots.Length; ++i)
        {
            m_Hotspots[i].Init(playerBehavior);
            BindComboSkillData(i, m_Hotspots[i], comboSequences);
        }
        m_Index = len == 0 ? -1 : 0;
    }

    private void BindComboSkillData(int index, AttackHotspot hotspot, ComboSequence[] combos)
    { 
        if(combos == null || combos.Length == 0 || hotspot == null || index < 0)
            return;

        for (int i = 0; i < combos.Length; ++i)
        {
            SkillData[] datas = combos[i].skillConfigs;
            for (int j = 0; j < datas.Length; ++j)
            {
                if (datas[j].attackHotspot == hotspot.name)
                    datas[j].hotspotIndex = index;
            }
        }
    }
}
