using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GameplayTag2 : System.IEquatable<GameplayTag2>
{
    public static readonly GameplayTag2 RootTag = new GameplayTag2("root");
    public static IEqualityComparer<GameplayTag2> EqualityComparer = new TagEqualityCompareImpl();

    [SerializeField] private string m_Name;

    public string name => m_Name;
    public int hash { get; }
    public bool isValid => !string.IsNullOrEmpty(name);

    public GameplayTag2(string tagName)
    {
        m_Name = tagName?.ToLowerInvariant();
        hash = m_Name?.GetHashCode() ?? 0;
    }

    public bool Equals(GameplayTag2 other) => hash == other.hash && name == other.name;
    public override bool Equals(object obj) => obj is GameplayTag2 other && Equals(other);
    public override int GetHashCode() => hash;
    public override string ToString() => name ?? "Invalid Tag";
    public static bool operator ==(GameplayTag2 left, GameplayTag2 right) => left.Equals(right);
    public static bool operator !=(GameplayTag2 left, GameplayTag2 right) => !left.Equals(right);


    public bool MatchesTag(GameplayTag2 other)
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

    public bool MatchesAny(GameplayTagContainer2 container)
    {
        foreach (var tag in container.tags)
        {
            if (MatchesTag(tag)) return true;
        }
        return false;
    }

    private class TagEqualityCompareImpl : IEqualityComparer<GameplayTag2>
    {
        public bool Equals(GameplayTag2 x, GameplayTag2 y) => x.Equals(y);
        public int GetHashCode(GameplayTag2 obj) => obj.hash;
    }
}