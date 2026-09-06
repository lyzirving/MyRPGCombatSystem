using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public partial struct GameplayTag : IEquatable<GameplayTag>
{
    public static readonly string RootName = "Root";
    public static readonly GameplayTag RootTag = new GameplayTag(RootName);

    public static IEqualityComparer<GameplayTag> EqualityComparer = new TagEqualityCompareImpl();

    [SerializeField] private string m_TagName;

    private int m_Index;
    private bool m_IsIndexCached;
    private string m_SimpleName;  
    
    public string simpleName
    {
        get
        {
            if (string.IsNullOrEmpty(m_SimpleName))
                m_SimpleName = m_TagName.Substring(m_TagName.IndexOf(".") + 1);
            return m_SimpleName;
        }
    }

    public GameplayTag(string tagName)
    {
        m_TagName = string.IsNullOrEmpty(tagName) ? string.Empty : tagName.Trim();
        m_Index = -1;
        m_IsIndexCached = false;
        m_SimpleName = string.Empty;
    }

    public GameplayTag(string tagName, int index)
    {
        m_TagName = string.IsNullOrEmpty(tagName) ? string.Empty : tagName.Trim();
        m_Index = index;
        m_IsIndexCached = m_Index > 0;
        m_SimpleName = string.Empty;
    }

    public int index
    {
        get
        {
            if (!m_IsIndexCached)
            {
                m_Index = GameplayTagManager.instance.GetIndex(m_TagName);
                m_IsIndexCached = true;
            }
            return m_Index;
        }
    }

    public bool isValid => index > 0;

    public int hash => index;

    public string name => m_TagName;

    public GameplayTag parent
    {
        get
        {
            int parentIndex = GameplayTagManager.instance.GetParent(index);
            return CreateTag(parentIndex);
        }
    }

    public GameplayTag[] children
    {
        get
        {
            int[] childIndices = GameplayTagManager.instance.GetChildIndices(index);
            var tags = new GameplayTag[childIndices.Length];
            for (int i = 0; i < childIndices.Length; i++)
                tags[i] = CreateTag(childIndices[i]);
            return tags;
        }
    }

    public override int GetHashCode() => index;
    public override string ToString() => isValid ? name : "EmptyTag";

    #region Equality
    public bool Equals(GameplayTag other) => index == other.index;
    public override bool Equals(object obj) => obj is GameplayTag other && Equals(other);    
    public static bool operator ==(GameplayTag left, GameplayTag right) => left.Equals(right);
    public static bool operator !=(GameplayTag left, GameplayTag right) => !left.Equals(right);
    #endregion

    #region Match Methods
    public bool Matches(GameplayTag other)
    {
        return GameplayTagManager.instance.Matches(this.index, other.index);
    }

    public bool MatchesAny(GameplayTagContainer container)
    {
        foreach (var idx in container.indices)
        {
            if (GameplayTagManager.instance.Matches(this.index, idx)) 
                return true;
        }
        return false;
    }
    #endregion

    #region Helper Methods
    public static GameplayTag CreateTag(int index)
    {
        if (index <= 0) 
            return RootTag;

        return new GameplayTag(GameplayTagManager.instance.GetName(index), index);
    }

    private class TagEqualityCompareImpl : IEqualityComparer<GameplayTag>
    {
        public bool Equals(GameplayTag x, GameplayTag y) => x.Equals(y);
        public int GetHashCode(GameplayTag obj) => obj.hash;
    }
    #endregion    
}