using UnityEngine;

public static class VectorExtension
{
    public static Vector3 NormalizeIgnoreY(this Vector3 filed)
    { 
        filed.y = 0;
        filed.Normalize();
        return filed;
    }
}
