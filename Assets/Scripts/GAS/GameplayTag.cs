using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GameplayTag : System.IEquatable<GameplayTag>
{
    public static readonly GameplayTag RootTag = new GameplayTag("root");
    public static IEqualityComparer<GameplayTag> EqualityComparer = new TagEqualityCompareImpl();

    [SerializeField] private string m_Name;

    public string name => m_Name;
    public int hash { get; }
    public bool isValid => !string.IsNullOrEmpty(name);

    public GameplayTag(string tagName)
    {
        m_Name = tagName?.ToLowerInvariant();
        hash = m_Name?.GetHashCode() ?? 0;
    }

    public bool Equals(GameplayTag other) => hash == other.hash && name == other.name;
    public override bool Equals(object obj) => obj is GameplayTag other && Equals(other);
    public override int GetHashCode() => hash;
    public override string ToString() => name ?? "Invalid Tag";
    public static bool operator ==(GameplayTag left, GameplayTag right) => left.Equals(right);
    public static bool operator !=(GameplayTag left, GameplayTag right) => !left.Equals(right);


    public bool MatchesTag(GameplayTag other)
    {
        if (!isValid || !other.isValid) return false;
        if (this == other) return true;
        
        var parents = GameplayTagManager.instance.GetParentTags(this);
        foreach (var parent in parents)
        {
            if (parent == other) return true;
        }
        return false;
    }

    public bool MatchesAny(GameplayTagContainer container)
    {
        foreach (var kvp in container.tags)
        {
            if (MatchesTag(kvp.Value)) return true;
        }
        return false;
    }

    private class TagEqualityCompareImpl : IEqualityComparer<GameplayTag>
    {
        public bool Equals(GameplayTag x, GameplayTag y) => x.Equals(y);
        public int GetHashCode(GameplayTag obj) => obj.hash;
    }
}