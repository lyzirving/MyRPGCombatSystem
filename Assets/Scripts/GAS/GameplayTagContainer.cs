using System;
using System.Collections.Generic;

[Serializable]
public class GameplayTagContainer
{
    private Dictionary<string, GameplayTag> m_Tags = new Dictionary<string, GameplayTag>();

    public IReadOnlyDictionary<string, GameplayTag> tags => m_Tags;

    public void Add(GameplayTag tag)
    {
        if (!tag.isValid)
            return;

        m_Tags[tag.name] = tag;
    }

    public bool Remove(GameplayTag tag)
    {
        return m_Tags.Remove(tag.name);
    }

    public bool Has(GameplayTag tag)
    {
        return m_Tags.ContainsKey(tag.name);
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

    public bool HasAny(IEnumerable<GameplayTag> tags)
    {
        if (tags == null) return false;

        var it = tags.GetEnumerator();
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
    }

    public void CopyFrom(GameplayTagContainer other)
    {
        m_Tags = new Dictionary<string, GameplayTag>(other.m_Tags);
    }
}
