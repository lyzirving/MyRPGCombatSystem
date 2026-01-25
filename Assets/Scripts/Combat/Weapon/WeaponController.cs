using UnityEngine;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    private Collider m_Collider;
    private HashSet<string> m_TagHashSet = new();
    [SerializeField] WeaponType m_WeaponType = WeaponType.UnArmed;

    public WeaponType type { get { return m_WeaponType; } }

#if UNITY_EDITOR
    [SerializeField] private List<string> m_ColliderTags = new();
#endif

    #region State Methods
    private void Awake()
    {
        m_Collider = GetComponent<Collider>();
        if (m_Collider == null)
            throw new System.Exception($"game object[{gameObject.name}] doesn't have a Collider");
    }

    private void OnTriggerStay(Collider other)
    {
    }
    #endregion

    #region Main Methods
    public void Init()
    { 
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
