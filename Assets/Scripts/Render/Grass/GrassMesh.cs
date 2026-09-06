using UnityEngine;

/// <summary>
/// Create mesh that is only layed on yz plane. Mesh on cpu is just a placeholder,
/// and shape of grass is actually renderer by BezierBlade.shader in gpu.
/// 
/// CubicBezier:
///  B(t) = (1-t)^3 * P0 + 3 * (1 -t)^2 * t * P1 + 3 * (1 - t) * t^2 * P2 + t^3 * P3.
/// P0 is the bottom of grass and P3 is the top of grass.
/// 
/// For mesh's color, R channel is a value among [0, 1], representing the height interpolation from 
/// grass bottom to grass top, typically the param t in the formula above.
/// G channel is used to mark vertex's side. 0: left side, 1: right side.
/// 
/// If grass is standing straight, it is entirly on yz plane.
/// If grass could bend, it would lean in the negative direction of the X-axis.
/// </summary>
public class GrassMesh
{
    public static Mesh CreateHighLODMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = new Vector3[]
            {                
                new Vector3(0.000000f, 0.15599f, 0.03445f),
                new Vector3(0.000000f, 0.00000f, -0.03444f),
                new Vector3(0.000000f, 0.00000f, 0.03444f),
                new Vector3(0.000000f, 0.15599f, -0.03445f),
                new Vector3(0.000000f, 0.27249f, -0.03193f),
                new Vector3(0.000000f, 0.27249f, 0.03193f),
                new Vector3(0.000000f, 0.38111f, -0.02942f),
                new Vector3(0.000000f, 0.38111f, 0.02942f),
                new Vector3(0.000000f, 0.47325f, -0.02620f),
                new Vector3(0.000000f, 0.47325f, 0.02620f),
                new Vector3(0.000000f, 0.55531f, -0.02338f),
                new Vector3(0.000000f, 0.55531f, 0.02338f),
                new Vector3(0.000000f, 0.63064f, -0.01728f),
                new Vector3(0.000000f, 0.63064f, 0.01728f),
                new Vector3(0.000000f, 0.70819f, 0.00000f)
            },

            triangles = new int[]
            {
                0, 1, 2,
                0, 3, 1,
                0, 4, 3,
                0, 5, 4,
                5, 6, 4,
                5, 7, 6,
                7, 8, 6,
                7, 9, 8,
                9, 10, 8,
                9, 11, 10,
                12, 10, 11,
                11, 13, 12,
                13, 14, 12
            },

            colors = new Color[]
            {
                new Color(0.141177f, 0.000000f, 0.000000f, 1.000000f),
                new Color(0.000000f, 1.000000f, 0.000000f, 1.000000f),
                new Color(0.000000f, 0.000000f, 0.000000f, 1.000000f),
                new Color(0.141177f, 1.000000f, 0.000000f, 1.000000f),
                new Color(0.286275f, 1.000000f, 0.000000f, 1.000000f),
                new Color(0.286275f, 0.000000f, 0.000000f, 1.000000f),
                new Color(0.427451f, 1.000000f, 0.000000f, 1.000000f),
                new Color(0.427451f, 0.000000f, 0.000000f, 1.000000f),
                new Color(0.572549f, 1.000000f, 0.000000f, 1.000000f),
                new Color(0.572549f, 0.000000f, 0.000000f, 1.000000f),
                new Color(0.713726f, 1.000000f, 0.000000f, 1.000000f),
                new Color(0.713726f, 0.000000f, 0.000000f, 1.000000f),
                new Color(0.858824f, 1.000000f, 0.000000f, 1.000000f),
                new Color(0.858824f, 0.000000f, 0.000000f, 1.000000f),
                new Color(1.000000f, 0.498039f, 0.000000f, 1.000000f)
            },

            uv = new Vector2[]
            {
                new Vector2(0.450011f, 0.220262f),
                new Vector2(0.550490f, 0.000000f),
                new Vector2(0.450038f, 0.000000f),
                new Vector2(0.550516f, 0.220262f),
                new Vector2(0.546832f, 0.354773f),
                new Vector2(0.453695f, 0.354773f),
                new Vector2(0.543177f, 0.508140f),
                new Vector2(0.457350f, 0.508140f),
                new Vector2(0.538472f, 0.628258f),
                new Vector2(0.462055f, 0.628258f),
                new Vector2(0.534360f, 0.744132f),
                new Vector2(0.466167f, 0.744132f),
                new Vector2(0.525474f, 0.850497f),
                new Vector2(0.475053f, 0.850497f),
                new Vector2(0.500264f, 0.90000f)
            }
        };

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }

    public static Mesh CreateLowLODMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = new Vector3[]
            {
                    new Vector3(0.000000f, 0.00000f, -0.03444f),  // bottom left
                    new Vector3(0.000000f, 0.00000f, 0.03444f),   // bottom right
                    new Vector3(0.000000f, 0.27249f, -0.03193f),  // middle bottom left
                    new Vector3(0.000000f, 0.27249f, 0.03193f),   // middle bottom right
                    new Vector3(0.000000f, 0.47325f, -0.02620f),  // middle upper left
                    new Vector3(0.000000f, 0.47325f, 0.02620f),   // middle upper right
                    new Vector3(0.000000f, 0.70819f, 0.00000f)    // top
            },

            triangles = new int[]
            {
                    1, 0, 3,       // first layer
                    0, 2, 3,
                    3, 2, 5,       // second layer
                    2, 4, 5,
                    5, 4, 6        // third layer
            },

            colors = new Color[]
            {
                    new Color(0.000000f, 1.000000f, 0.000000f, 1.000000f),
                    new Color(0.000000f, 0.000000f, 0.000000f, 1.000000f),
                    new Color(0.286275f, 1.000000f, 0.000000f, 1.000000f),
                    new Color(0.286275f, 0.000000f, 0.000000f, 1.000000f),
                    new Color(0.572549f, 1.000000f, 0.000000f, 1.000000f),
                    new Color(0.572549f, 0.000000f, 0.000000f, 1.000000f),
                    new Color(1.000000f, 0.498039f, 0.000000f, 1.000000f)
            },

            uv = new Vector2[]
            {
                    new Vector2(0.550490f, 0.000000f),
                    new Vector2(0.450038f, 0.000000f),
                    new Vector2(0.546832f, 0.354773f),
                    new Vector2(0.453695f, 0.354773f),
                    new Vector2(0.538472f, 0.628258f),
                    new Vector2(0.462055f, 0.628258f),
                    new Vector2(0.500264f, 0.90000f)
            }
        };

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }
}