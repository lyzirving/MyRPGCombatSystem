using UnityEngine;

namespace GameConsts
{
    public static class Layer
    {
        public static readonly LayerMask All = int.MaxValue;
        // Get in runtime
        public static readonly LayerMask Walkable = LayerMask.GetMask("Walkable");
    }      
}

public static class GameUtility
{
    public static bool ContainsLayer(LayerMask? mask, int layer)
    {
        return mask.HasValue && (((1 << layer) & mask) != 0);
    }

    public static bool IsWalkableLayer(int layer)
    {
        return ContainsLayer(GameConsts.Layer.Walkable, layer);
    }
}