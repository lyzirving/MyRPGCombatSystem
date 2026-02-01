using UnityEngine;
using System.Collections.Generic;

public class AttackHotspot : MonoBehaviour
{
    private Collider m_Collider;
    private HashSet<string> m_TagHashSet = new();
    private Dictionary<int, ISkillTarget> m_HitTargets = new Dictionary<int, ISkillTarget>();
    private IPlayerBehavior m_PlayerBehavior;

    public SkillData skillConfig;

#if UNITY_EDITOR
    [SerializeField] private List<string> m_ColliderTags = new();
#endif

    #region State Methods
    private void Awake()
    {
        m_Collider = GetComponent<Collider>();
        if (m_Collider == null)
            throw new System.Exception("Fail to find Collider on GameObject");
    }

    private void OnDestroy()
    {
        m_PlayerBehavior = null;
    }

    private void OnTriggerStay(Collider other)
    {
        if(!m_TagHashSet.Contains(other.gameObject.tag))
            return;

        if (m_HitTargets.ContainsKey(other.GetInstanceID()))
        {
            // Only effect the GameObject once during one attack
            // So we should do nothing in this case
        }
        else 
        {
            var comp = other.GetComponentInParent<ISkillTarget>();
            if (comp != null)
            {
                m_HitTargets.Add(other.GetInstanceID(), comp);
                m_PlayerBehavior?.OnAttackHit(skillConfig, comp, other.ClosestPoint(transform.position));
            }
        }
    }
    #endregion

    #region Main Methods
    public void Init(IPlayerBehavior playerBehavior)
    {
        m_PlayerBehavior = playerBehavior;
    }

    public void OnStartAttack()
    {
        if (m_Collider == null)
            return;

        m_Collider.enabled = true;
        m_HitTargets.Clear();
    }

    public void OnStopAttack()
    {
        if (m_Collider == null)
            return;

        m_Collider.enabled = false;
    }
    #endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        SyncSerializedTags();
    }

    private void SyncSerializedTags()
    {
        m_TagHashSet.Clear();
        for (int i = 0; i < m_ColliderTags.Count; ++i)
        {
            m_TagHashSet.Add(m_ColliderTags[i]);
        }
    }
#endif
}
