using System.Collections.Generic;

public class GameplayTag
{
    public string name;

    public bool isValid => !string.IsNullOrEmpty(name);

    public GameplayTag(string name)
    {
        this.name = name.ToLower();
    }

    public bool Matches(GameplayTag other)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(other.name))
            return false;

        return name == other.name;
    }

    public bool MatchesAny(IEnumerable<GameplayTag> tags)
    {    
        if (tags == null)
            return false;

        var it = tags.GetEnumerator();
        while (it.MoveNext())
        {
            if(Matches(it.Current))
                return true;
        }
        it.Dispose();

        return false;
    }

    public override string ToString() => name;       
}
