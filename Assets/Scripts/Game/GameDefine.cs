using UnityEngine;

public class GameConsts
{
    // Get in runtime
    public static readonly LayerMask WalkableLayer = LayerMask.GetMask("Walkable");
}

public class GameUtility
{
    public static bool ContainsLayer(LayerMask? mask, int layer)
    {
        return mask.HasValue && (((1 << layer) & mask) != 0);
    }

    public static bool IsWalkableLayer(int layer)
    {
        return ContainsLayer(GameConsts.WalkableLayer, layer);
    }
}