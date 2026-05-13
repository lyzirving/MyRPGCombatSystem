
[System.Serializable]
public struct GameplayTagContainer2
{
    public GameplayTag2[] tags;

    public bool HasTag(GameplayTag2 tagToCheck)
    {
        foreach (var tag in tags)
            if (tag == tagToCheck) return true;

        return false;
    }
    public bool HasTagExact(GameplayTag2 tagToCheck) => HasTag(tagToCheck);
    public bool HasAny(GameplayTagContainer2 other)
    {
        foreach (var myTag in tags)
            foreach (var otherTag in other.tags)
                if (myTag.MatchesTag(otherTag)) return true;
        return false;
    }
}
