using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class GUIDDataBase : ScriptableObject
{
    [SerializeField] private List<GUIDEntry> m_GuidEntries = new List<GUIDEntry>();

    [SerializeField] private string m_LastUpdateTime;
    [SerializeField] private int m_CurrentId = 0;

    private Queue<int> m_AvailableGuids = new Queue<int>();

    public List<GUIDEntry> allEntries => m_GuidEntries;
    public int count => m_GuidEntries.Count;
    public string modifyTime => m_LastUpdateTime;

    public GUIDEntry AddGUID(string name = "", string category = "default", string descriptioin = "null")
    {
        GUIDEntry entry = new GUIDEntry();

        entry.guid = m_AvailableGuids.Count == 0 ? m_CurrentId++ : m_AvailableGuids.Dequeue();
        entry.name = name;
        entry.category = category;
        entry.description = descriptioin;
        entry.createdTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // need thread safe?
        m_GuidEntries.Add(entry);

        UpdateTimestamp();
        return entry;
    }

    public GUIDEntry FindByGuid(int guid)
    {
        return m_GuidEntries.Find(e => e.guid == guid);
    }

    public GUIDEntry FindByName(string name)
    {
        return m_GuidEntries.Find(e => e.name == name);
    }

    public bool RemoveByGuid(int guid)
    {
        int index = m_GuidEntries.FindIndex(e => e.guid == guid);
        if (index >= 0)
        {
            m_AvailableGuids.Enqueue(guid);
            m_GuidEntries.RemoveAt(index);
            UpdateTimestamp();
            return true;
        }
        return false;
    }

    public bool UpdateEntry(int guid, string newName = null, string newCategory = null, string newDecription = null)
    {        
        GUIDEntry entry = FindByGuid(guid);
        if (entry == null) return false;

        bool modified = false;

        if (!string.IsNullOrEmpty(newName))
        {
            entry.name = newName;
            modified = true;
        }

        if (!string.IsNullOrEmpty(newCategory))
        {
            entry.category = newCategory;
            modified = true;
        }

        if (!string.IsNullOrEmpty(newDecription))
        {
            entry.description = newDecription;
            modified = true;
        }

        if (modified) 
            UpdateTimestamp();

        return modified;
    }

    public void ClearAll()
    {
        m_CurrentId = 0;
        m_AvailableGuids.Clear();
        m_GuidEntries.Clear();
        UpdateTimestamp();
    }

    private void UpdateTimestamp()
    {
        m_LastUpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }
}
