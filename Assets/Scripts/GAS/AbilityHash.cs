using UnityEngine;

public static class AbilityHash<T> where T : GameplayAbility
{
    public static readonly int classHash = typeof(T).GUID.GetHashCode();
}
