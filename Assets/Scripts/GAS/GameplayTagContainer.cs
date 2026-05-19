using System;
using System.Collections.Generic;

[Serializable]
public class GameplayTagContainer
{
    private List<string> m_Tags = new List<string>();
    private int[] m_Indices = new int[0];
    private bool m_Dirty = true;

    public int[] indices
    {
        get
        {
            if (m_Dirty) 
                RefreshIndices();
            return m_Indices;
        }
    }

    public int count => indices.Length;    

    public void Add(GameplayTag tag)
    {
        if (!tag.isValid)
            return;

        string name = tag.name;
        if (!m_Tags.Contains(name))
            m_Tags.Add(name);
        m_Dirty = true;
    }

    public bool Remove(GameplayTag tag)
    {
        bool ret = m_Tags.Remove(tag.name);
        if (ret) 
            m_Dirty = true;
        return ret;
    }

    /// <summary>
    /// Check whether container contains the tag. It will check the source's parent chain.
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public bool Has(GameplayTag tag)
    {
        var target = tag.index;
        if (target <= 0)
            return false;

        var indList = indices;
        foreach (int i in indList)
        {
            if (GameplayTagManager.instance.Matches(i, target))
                return true;
        }
        return false;
    }

    public bool Has(string name)
    {
        var target = GameplayTagManager.instance.GetIndex(name);
        if(target <= 0)
            return false;

        var indList = indices;
        foreach (int i in indList)
        {
            if (GameplayTagManager.instance.Matches(i, target))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Check whether container directly contains the tag
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public bool HasExact(GameplayTag tag)
    {
        int target = tag.index;
        if (target == 0) 
            return false;
        var ind = indices;

        foreach (int i in ind)
            if (i == target) 
                return true;

        return false;
    }

    public bool HasExact(string name)
    {
        if(string.IsNullOrEmpty(name))
            return false;

        foreach (var tag in m_Tags)
            if (tag == name)
                return true;

        return false;
    }

    public bool HasAll(IEnumerable<GameplayTag> tags)
    {
        if (tags == null) return false;

        var it = tags.GetEnumerator();
        while (it.MoveNext())
        {
            if(!Has(it.Current))
                return false;
        }
        return true;
    }

    public bool HasAll(GameplayTagContainer container)
    {
        if (container == null || container.m_Tags == null) 
            return false;

        var it = container.m_Tags.GetEnumerator();
        while (it.MoveNext())
        {
            if (!Has(it.Current))
                return false;
        }
        return true;
    }

    public bool HasAny(IEnumerable<GameplayTag> tags)
    {
        if (tags == null) 
            return false;

        var it = tags.GetEnumerator();
        while (it.MoveNext())
        {
            if (Has(it.Current))
                return true;
        }
        return false;
    }

    public bool HasAny(GameplayTagContainer container)
    {
        if (container == null || container.m_Tags == null)
            return false;

        var it = container.m_Tags.GetEnumerator();
        while (it.MoveNext())
        {
            if (Has(it.Current))
                return true;
        }
        return false;
    }

    public void Clear()
    {
        m_Tags.Clear();
        m_Dirty = true;
    }

    public void CopyFrom(GameplayTagContainer other)
    {
        m_Tags = new List<string>(other.m_Tags);
        m_Dirty = true;
    }

    private void RefreshIndices()
    {
        List<int> temp = new List<int>();
        foreach (string tag in m_Tags)
        {
            int idx = GameplayTagManager.instance.GetIndex(tag);
            if (idx > 0)
                temp.Add(idx);
            else
                throw new Exception($"invalid tag[{tag}] and index[{idx}]");
        }
        m_Indices = temp.ToArray();
        m_Dirty = false;
    }
}
