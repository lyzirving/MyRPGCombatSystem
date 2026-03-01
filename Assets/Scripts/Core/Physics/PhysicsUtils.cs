using System;

public class PhysicsUtils
{
    /// <summary>
    /// v1^2 - v0^2 = 2*a*s
    /// </summary>
    /// <param name="v0">initial speed</param>
    /// <param name="a">acceleration</param>
    /// <param name="s">distance from v0 to v1 accelerated by a</param>
    /// <returns></returns>
    public static float CalcTargetVelocity(float v0, float a, float s)
    {
        if (a < 0) a = -a;
        if (s < 0) s = -s;
        return (float)Math.Sqrt(v0 * v0 + 2f * a * s);
    }
}