using UnityEngine;

public class AttackComponent : MonoBehaviour
{
    [SerializeField] private ComboSequence[] m_ComboSequences;

    private ComboController m_Controller = new();
    private AttackBox[] m_AttackBox;  

    public bool hasSkill => m_ComboSequences != null && m_ComboSequences.Length > 0;
    public bool hasAttackBox => m_AttackBox != null && m_AttackBox.Length > 0;
    public bool hasNextSkill => m_Controller.hasNextSkill;

    public ComboSequence combo { get => m_ComboSequences[m_Controller.comboIndex]; }
    public SkillData skill { get => combo.skillConfigs[m_Controller.skillIndex]; }
    public AttackBox attackBox { get => m_AttackBox[skill.attackBoxIndex]; }

    #region State Methods
    private void OnDestroy()
    {
        m_Controller.DeInit();
    }
    #endregion

    #region Main Method
    public void Init(ICharacterBehavior playerBehavior)
    {
        m_Controller.Init(playerBehavior, m_ComboSequences);

        m_AttackBox = GetComponentsInChildren<AttackBox>();
        int len = m_AttackBox == null ? 0 : m_AttackBox.Length;
        if (len != 0)
        {
            CreateSkillHotspotIndex(playerBehavior);
            CrateSkillConnection();
        }
        else
        {
            Debug.LogWarning($"No attack hotspots is bound in go[{this.gameObject}]");     
        }        
    }

    private void CreateSkillHotspotIndex(ICharacterBehavior playerBehavior)
    {
        if (m_ComboSequences == null || m_ComboSequences.Length == 0)
        {
            Debug.LogWarning("invalid combo sequence");
            return;
        }

        if (m_AttackBox == null || m_AttackBox.Length == 0)
        {
            Debug.LogWarning("invalid attack box on player");
            return;
        }

        for (int index = 0; index < m_AttackBox.Length; ++index)
        {
            var attackBox = m_AttackBox[index];
            attackBox.Init(playerBehavior);

            for (int m = 0; m < m_ComboSequences.Length; ++m)
            {
                SkillData[] skills = m_ComboSequences[m].skillConfigs;
                if (skills == null)
                    continue;

                for (int n = 0; n < skills.Length; ++n)
                {
                    if (skills[n].attackBox == attackBox.name)
                        skills[n].attackBoxIndex = index;
                }
            }
        }
    }

    private void CrateSkillConnection()
    {
        if (m_ComboSequences == null || m_ComboSequences.Length == 0)
        {
            Debug.LogWarning("invalid combo sequence");
            return;
        }

        for (int comboIndex = 0; comboIndex < m_ComboSequences.Length; ++comboIndex)
        {
            var combo = m_ComboSequences[comboIndex];
            if (combo.skillConfigs == null)
                continue;

            for (int skillIndex = 1; skillIndex < combo.skillConfigs.Length; ++skillIndex)
            {
                combo.skillConfigs[skillIndex - 1].nextSkillIndex = skillIndex;
            }
            combo.lastSkillConfig.nextSkillIndex = -1;
        }
    }
    #endregion

    #region Combo Method
    public void BeginCombo()
    {
        m_Controller.BeginCombo();
    }

    public bool GoNextSkill()
    {
        return m_Controller.GoNextSkill();
    }

    public void NextSkill()
    {
        m_Controller.NextSkill();
    }

    public void EndCombo()
    {
        m_Controller.EndCombo();
    }
    #endregion
}
