using UnityEngine;

public static class Bezier
{
    public static Vector3 Sample(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    { 
        t = Mathf.Clamp01(t);
        float oneMinusT = 1 - t;
        return oneMinusT * oneMinusT * oneMinusT * p0 +
            3 * oneMinusT * oneMinusT * t * p1 +
            3 * oneMinusT * t * t * p2 +
            t * t * t * p3;
    }

    public static Vector3 Tangent(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1 - t;
        return 3f * oneMinusT * oneMinusT * (p1 - p0) +
            6 * oneMinusT * t * (p2 - p1) +
            3 * t * t * (p3 - p2);
    }
}
