using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System;

/// <summary>
/// Global GameplayTag manager, which is responsible for tag registration, node building, and parent-child relationship building
/// </summary>
public class GameplayTagManager : Singleton<GameplayTagManager>
{
    public const string path = "GAS/GlobalGameplayTagDatabase";
    public IReadOnlyList<GameplayTag> tags => m_AllTags;
    
    private bool m_IsLoaded = false;
    private List<GameplayTag> m_AllTags = new List<GameplayTag>();

    // Index 0 is always for root tag.
    private int m_CurrentIndex = 0;
    private Dictionary<string, int> m_NameToIndex = new Dictionary<string, int>();
    private Dictionary<int, string> m_IndexToName = new Dictionary<int, string>();
    private Dictionary<int, int> m_IndexToParent = new Dictionary<int, int>();
    private Dictionary<int, List<int>> m_IndexToChildren = new Dictionary<int, List<int>>();
    // int[] stores parent index, the first should always be self, and then direct parent, the last should be root.
    private Dictionary<int, int[]> m_IndexToParentChain = new Dictionary<int, int[]>();

    public bool isLoaded => m_IsLoaded;

    #region Main Methods
    public override void OnInit()
    {
        LoadGameplayTags(true);
    }

    public void LoadGameplayTags(bool force = false)
    {
        if(m_IsLoaded && !force)
            return;

        var handle = Addressables.LoadAssetAsync<GameplayTagDatabase>(path);
        var database = handle.WaitForCompletion();
        if (database == null)
            throw new Exception($"Fail to find GameplayTagDatabase at: {path}");

        CreateTagIndex(database.tags);

        handle.Release();
    }

    public void CreateTagIndex(List<GameplayTag> tags)
    {
        Clear();

        m_IndexToName[0] = GameplayTag.RootTag.name;
        m_NameToIndex[GameplayTag.RootTag.name] = 0;
        m_CurrentIndex = 1;

        AddTagIndex(tags);

        m_IsLoaded = true;
    }

    public void AddTagIndex(List<GameplayTag> tags)
    {
        if (tags == null || tags.Count == 0)
            return;

        for (int i = 0; i < tags.Count; i++)
        {
            GameplayTag t = tags[i];
            var name = t.name;
            if (string.IsNullOrEmpty(name))
                continue;

            name = name.Trim();
            if (m_NameToIndex.ContainsKey(name))
                continue;

            m_IndexToName[m_CurrentIndex] = name;
            m_NameToIndex[name] = m_CurrentIndex;
            ++m_CurrentIndex;

            m_AllTags.Add(t);
        }        

        BuildHierarchy();
    }    

    public void Clear()
    {
        m_IsLoaded = false;
        m_AllTags.Clear();
        m_IndexToName.Clear();
        m_IndexToParent.Clear();
        m_NameToIndex.Clear();
        m_IndexToChildren.Clear();
        m_IndexToParentChain.Clear();
    }
    #endregion

    #region Build Index Methods
    private void BuildHierarchy()
    {
        foreach (var kvp in m_NameToIndex)
        {
            string tagName = kvp.Key;
            int index = kvp.Value;

            if (index == 0) 
                continue;

            int lastDot = tagName.LastIndexOf('.');
            string parentName = lastDot > 0 ? tagName.Substring(0, lastDot) : GameplayTag.RootName;
            int parentIndex = GetIndex(parentName);

            m_IndexToParent[index] = parentIndex;
            if (!m_IndexToChildren.ContainsKey(parentIndex))
                m_IndexToChildren[parentIndex] = new List<int>();

            int ret = m_IndexToChildren[parentIndex].FindIndex(i => i == index);
            if(ret == -1)
                m_IndexToChildren[parentIndex].Add(index);
        }

        foreach (int index in m_IndexToName.Keys)
        {
            m_IndexToParentChain[index] = BuildParentChain(index);
        }
    }

    private int[] BuildParentChain(int index)
    {
        List<int> chain = new List<int>();
        int current = index;
        while (true)
        {
            chain.Add(current);
            if (current == 0)
                break;
            current = m_IndexToParent.TryGetValue(current, out int parent) ? parent : 0;                       
        }
        return chain.ToArray();
    }
    #endregion

    #region Index Query
    public int GetIndex(string name)
    {
        if (string.IsNullOrEmpty(name)) 
            return 0;

        return m_NameToIndex.TryGetValue(name.Trim(), out int index) ? index : 0;
    }

    public string GetName(int index)
    {
        return m_IndexToName.TryGetValue(index, out string name) ? name : GameplayTag.RootName;
    }

    public int GetParent(int index)
    {
        return m_IndexToParent.TryGetValue(index, out int parent) ? parent : 0;
    }

    public int[] GetParentChain(int index)
    {
        return m_IndexToParentChain.TryGetValue(index, out int[] chain) ? chain : new int[] { 0 };
    }

    public int[] GetChildIndices(int index)
    {
        return m_IndexToChildren.TryGetValue(index, out var list) ? list.ToArray() : new int[0];
    }
    #endregion

    #region Tag Query
    public bool Matches(int source, int target)
    {
        if (source == 0 || target == 0) 
            return false;

        int[] chain = GetParentChain(source);
        for (int i = 0; i < chain.Length; i++)
        {
            if (chain[i] == target)
                return true;
        }
        return false;
    }

    public GameplayTag GetTag(string name)
    {
        if(string.IsNullOrEmpty(name))
            return GameplayTag.RootTag;

        if (!m_NameToIndex.TryGetValue(name.Trim(), out int idx))
            return GameplayTag.RootTag;
        return GameplayTag.CreateTag(idx);
    }

    public bool HasTag(GameplayTag gameplayTag)
    {
        return gameplayTag.isValid;
    }
    #endregion

    #region Tag Operation
    public bool AddTag(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        var normalized = name.Trim();
        if (m_NameToIndex.ContainsKey(normalized))
            return false;

        int idx = m_CurrentIndex++;
        m_IndexToName[idx] = normalized;
        m_NameToIndex[normalized] = idx;

        GameplayTag tag = new GameplayTag(normalized);
        m_AllTags.Add(tag);

        int lastDot = normalized.LastIndexOf('.');
        string parentName = lastDot > 0 ? normalized.Substring(0, lastDot) : GameplayTag.RootName;
        int parentIndex = GetIndex(parentName);

        m_IndexToParent[idx] = parentIndex;
        if (!m_IndexToChildren.ContainsKey(parentIndex))
            m_IndexToChildren[parentIndex] = new List<int>();

        int ret = m_IndexToChildren[parentIndex].FindIndex(i => i == idx);
        if (ret == -1)
            m_IndexToChildren[parentIndex].Add(idx);

        m_IndexToParentChain.Add(idx, BuildParentChain(idx));

        return true;
    }

    public bool RemoveTag(string name)
    { 
        if(string.IsNullOrEmpty(name))
            return false;

        if (m_NameToIndex.TryGetValue(name, out int idx))
        {
            RemoveTag(idx);
            return true;
        }

        return false;
    }

    public void RemoveTag(int index)
    {        
        if (m_IndexToChildren.TryGetValue(index, out var children))
        { 
            for (int i = 0; i < children.Count; ++i)
                RemoveTag(children[i]);
        }

        if (m_IndexToParent.TryGetValue(index, out int parent) && 
            m_IndexToChildren.TryGetValue(parent, out var parentChildList))
        {
            parentChildList.Remove(index);
        }

        if (m_IndexToName.TryGetValue(index, out var name))
            m_NameToIndex.Remove(name);

        m_IndexToName.Remove(index);
        m_IndexToParentChain.Remove(index);
        m_IndexToParent.Remove(index);
        m_IndexToChildren.Remove(index);
    }

    public bool ChangeTagName(string oldName, string newName)
    {        
        if (m_NameToIndex.TryGetValue(oldName, out int idx))
        {
            m_NameToIndex.Remove(oldName);
            if (m_NameToIndex.ContainsKey(newName))
                m_NameToIndex[newName] = idx;
            else
                m_NameToIndex.Add(newName, idx);
            m_IndexToName[idx] = newName;
            return true;
        }
        return false;
    }
    #endregion
}
