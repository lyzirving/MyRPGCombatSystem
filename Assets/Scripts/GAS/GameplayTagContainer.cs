using System.Collections.Generic;
using System;

[Serializable]
public class GameplayTagContainer
{
    private Dictionary<string, GameplayTag> m_Tags = new Dictionary<string, GameplayTag>();

    public IReadOnlyDictionary<string, GameplayTag> tags => m_Tags;

    public void AddTag(GameplayTag tag)
    {
        if (!HasTag(tag))
        {
            m_Tags.Add(tag.name, tag);
        }
    }

    public bool RemoveTag(GameplayTag tag)
    {
        return m_Tags.Remove(tag.name);
    }

    public bool HasTag(GameplayTag tag)
    {
        if(tag == null || !tag.isValid)
            return false;

        return m_Tags.ContainsKey(tag.name);
    }

    public bool HasAllTags(IEnumerable<GameplayTag> tags)
    {
        if (tags == null) return false;

        var it = tags.GetEnumerator();
        while (it.MoveNext())
        {
            if(!HasTag(it.Current))
                return false;
        }
        return true;
    }

    public bool HasAnyTag(IEnumerable<GameplayTag> tags)
    {
        if (tags == null) return false;

        var it = tags.GetEnumerator();
        while (it.MoveNext())
        {
            if (HasTag(it.Current))
                return true;
        }
        return false;
    }

    public void Clear()
    {
        m_Tags.Clear();
    }

    public void CopyFrom(GameplayTagContainer other)
    {
        m_Tags = new Dictionary<string, GameplayTag>(other.m_Tags);
    }
}
