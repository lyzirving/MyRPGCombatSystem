using UnityEngine;

public enum EDistanceZone
{
    None,
    CloseCombatRange,
    Close,
    Mid,
    Far,
    Num
}

[CreateAssetMenu(fileName = "DistanceZoneSettings", menuName = "Config/DistanceZoneSettings")]
public class DistanceZoneSettings : ScriptableObject
{
    public float closeCombatRange = 1.2f;
    public float closeRange = 3f;
    public float midRange = 8f;

    public bool WithinRange(EDistanceZone zone, float distance)
    {
        switch (zone)
        {
            case EDistanceZone.CloseCombatRange: return distance <= closeCombatRange;
            case EDistanceZone.Close: return distance > closeCombatRange && distance <= closeRange;
            case EDistanceZone.Mid: return distance > closeRange && distance <= midRange;
            case EDistanceZone.Far: return distance > midRange;
            default: return false;
        }
    }

    public EDistanceZone GetZone(float distance)
    {
        if (distance <= closeCombatRange)
            return EDistanceZone.CloseCombatRange;
        else if (distance <= closeRange)
            return EDistanceZone.Close;
        else if (distance <= midRange)
            return EDistanceZone.Mid;
        else
            return EDistanceZone.Far;
    }
}
