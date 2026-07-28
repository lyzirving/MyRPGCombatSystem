using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AttackBox : MonoBehaviour
{
    private Collider m_Collider;
    private Collider[] m_ChildColliders;
    private HashSet<string> m_TagHashSet = new();
    private Dictionary<int, ICharacterBehavior> m_HitTargets = new Dictionary<int, ICharacterBehavior>();
    private ICharacterBehavior m_PlayerBehavior;

#if UNITY_EDITOR
    [SerializeField] private List<string> m_ColliderTags = new();
#endif

    #region State Methods
    private void Awake()
    {
        m_Collider = GetComponent<Collider>();
        if (m_Collider == null)
            throw new System.Exception("Fail to find Collider on GameObject");
        m_ChildColliders = GetComponentsInChildren<Collider>();
    }

    private void OnDestroy()
    {
        m_PlayerBehavior = null;
        m_ChildColliders = null;
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
            var target = other.GetComponentInParent<ICharacterBehavior>();
            if (target != null)
            {
                m_HitTargets.Add(other.GetInstanceID(), target);
                m_PlayerBehavior?.OnAttackHit(target, other.ClosestPoint(transform.position));
            }
        }
    }
    #endregion

    #region Main Methods
    public void Init(ICharacterBehavior playerBehavior)
    {
        m_PlayerBehavior = playerBehavior;
    }

    public void OnAttackBegin()
    {
        if (m_Collider == null)
            return;

        m_Collider.enabled = true;
        EnableChildColliders(true);
        m_HitTargets.Clear();
    }

    public void OnAttackEnd()
    {
        if (m_Collider == null)
            return;

        EnableChildColliders(false);
        m_Collider.enabled = false;
    }

    public void EnableChildColliders(bool enable)
    {
        if(m_ChildColliders == null || m_ChildColliders.Length == 0) return;
        for(int i = 0; i < m_ChildColliders.Length; i++)
            m_ChildColliders[i].enabled = enable;
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
