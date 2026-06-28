using UnityEngine;
using System.Collections.Generic;

public class LineBuilder : MonoBehaviour
{
    public enum UVExtend
    {
        ExtendAlongU,
        ExtendAlongV,
    }

    public List<Transform> points = new List<Transform>();
    public float width = 0.1f;
    public float miterLimit = 2f;
    public Material material = null;

    private Mesh m_Mesh = null;

    private void Awake()
    {
        m_Mesh = new Mesh();
        m_Mesh.MarkDynamic();
    }

    private void LateUpdate()
    {
        if (m_Mesh == null || m_Mesh.vertexCount == 0)
            return;

        Graphics.DrawMesh(m_Mesh, Matrix4x4.identity, material, gameObject.layer);
    }

    [ContextMenu("Build Mesh")]
    private void Build()
    {
        Vector3[] pointList = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
            pointList[i] = points[i].position;
        BuildLineMesh(pointList, width, miterLimit, UVExtend.ExtendAlongV, m_Mesh);
    }
    
    public static bool BuildLineMesh(Vector3[] pointList, float w, float miterLimit, UVExtend uvExt, Mesh mesh)
    {
        int count = pointList == null ? 0 : pointList.Length;
        if (count < 2)
            return false;

        Vector3 up = Vector3.up;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        Vector3[] tangents = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            if (i == 0)
                tangents[i] = (pointList[1] - pointList[0]).normalized;
            else if (i == count - 1)
                tangents[i] = (pointList[count - 1] - pointList[count - 2]).normalized;
            else
                tangents[i] = (pointList[i + 1] - pointList[i - 1]).normalized;
        }

        Vector3 firstLeft = Vector3.Cross(tangents[0], up).normalized;
        Vector3 firstLeftPt = pointList[0] + firstLeft * w;
        Vector3 firstRightPt = pointList[0] - firstLeft * w;

        int leftIdx = vertices.Count;
        vertices.Add(firstLeftPt);
        int rightIdx = vertices.Count;
        vertices.Add(firstRightPt);
        if (uvExt == UVExtend.ExtendAlongU)
        {
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 0));
        }
        else
        {
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
        }
        

        Vector3 prevLeft = firstLeft;
        int prevLeftIdx = leftIdx;
        int prevRightIdx = rightIdx;

        for (int i = 1; i < count - 1; i++)
        {
            Vector3 tangent = tangents[i];
            Vector3 left = Vector3.Cross(tangent, up).normalized;

            // raw position without Miter
            Vector3 leftPt = pointList[i] + left * w;
            Vector3 rightPt = pointList[i] - left * w;

            // calculate two continous segments' angle
            Vector3 prevTangent = tangents[i - 1];
            float dot = Vector3.Dot(prevLeft, left);
            float angleRad = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f));
            float angle = angleRad * Mathf.Rad2Deg;

            // Nearly straight line
            if (angle < 0.01f)
            {
                int newLeft = vertices.Count;
                vertices.Add(leftPt);
                int newRight = vertices.Count;
                vertices.Add(rightPt);
                if (uvExt == UVExtend.ExtendAlongU)
                {
                    uvs.Add(new Vector2(1f - (float)i / count, 1f));
                    uvs.Add(new Vector2(1f - (float)i / count, 0f));
                }
                else
                {
                    uvs.Add(new Vector2(0, (float)i / count));
                    uvs.Add(new Vector2(1, (float)i / count));
                }                

                AddQuad(triangles, prevLeftIdx, prevRightIdx, newLeft, newRight);
                prevLeftIdx = newLeft;
                prevRightIdx = newRight;
                prevLeft = left;
                continue;
            }

            // Miter
            Vector3 miterDir = (prevLeft + left).normalized;
            float miterLength = 1f / Mathf.Sin(angleRad * 0.5f);

            // When the corner is too sharp.
            if (miterLength > miterLimit)
            {
                // ========== Bevel 连接 ==========
                // 生成当前路径点的两对顶点：
                //   - 前段左右点：使用前一段的侧向量 prevRight，宽度用当前点宽度
                //   - 后段左右点：使用当前段的侧向量 right，宽度用当前点宽度
                // 这样在转角处会形成一个切角，避免尖刺。

                Vector3 bevelLeftPrev = pointList[i] + prevLeft * w;
                Vector3 bevelRightPrev = pointList[i] - prevLeft * w;

                Vector3 bevelLeftCurr = pointList[i] + left * w;
                Vector3 bevelRightCurr = pointList[i] - left * w;

                int blpIdx = vertices.Count;
                vertices.Add(bevelLeftPrev);
                int brpIdx = vertices.Count;
                vertices.Add(bevelRightPrev);
                int blcIdx = vertices.Count;
                vertices.Add(bevelLeftCurr);
                int brcIdx = vertices.Count;
                vertices.Add(bevelRightCurr);

                float v = (float)i / count;                
                if (uvExt == UVExtend.ExtendAlongU)
                {
                    uvs.Add(new Vector2(1f - v, 1f));
                    uvs.Add(new Vector2(1f - v, 0f));
                    uvs.Add(new Vector2(1f - v, 1f));
                    uvs.Add(new Vector2(1f - v, 0f));
                }
                else
                {
                    uvs.Add(new Vector2(0, v));
                    uvs.Add(new Vector2(1, v));
                    uvs.Add(new Vector2(0, v));
                    uvs.Add(new Vector2(1, v));
                }

                AddQuad(triangles, prevLeftIdx, prevRightIdx, blpIdx, brpIdx);
                AddQuad(triangles, blpIdx, brpIdx, blcIdx, brcIdx);

                prevLeftIdx = blcIdx;
                prevRightIdx = brcIdx;
                prevLeft = left;

                continue;
            }

            float miterScale = 1f / Mathf.Max(0.01f, Vector3.Dot(prevLeft, miterDir));
            Vector3 miterLeft = pointList[i] + miterDir * (w * miterScale);
            Vector3 miterRight = pointList[i] - miterDir * (w * miterScale);

            Vector3 interLeft, interRight;
            if (LineLineIntersection(
                pointList[i - 1] + prevLeft * w, prevTangent,
                pointList[i] + left * w, tangent,
                out interLeft))
            {
                // LineLineIntersection will calculate interLeft, and use it as Miter
            }
            else
            {
                // Fallback when parallel
                interLeft = leftPt;
            }

            if (LineLineIntersection(
                pointList[i - 1] - prevLeft * w, prevTangent,
                pointList[i] - left * w, tangent,
                out interRight))
            {
                // LineLineIntersection will calculate interRight, and use it as Miter
            }
            else
            {
                // Fallback when parallel
                interRight = rightPt;
            }

            int miterLeftIdx = vertices.Count;
            vertices.Add(interLeft);
            int miterRightIdx = vertices.Count;
            vertices.Add(interRight);
            if (uvExt == UVExtend.ExtendAlongU)
            {
                uvs.Add(new Vector2(1f - (float)i / count, 1f));
                uvs.Add(new Vector2(1f - (float)i / count, 0f));
            }
            else
            {
                uvs.Add(new Vector2(0, (float)i / count));
                uvs.Add(new Vector2(1, (float)i / count));
            }

            AddQuad(triangles, prevLeftIdx, prevRightIdx, miterLeftIdx, miterRightIdx);
            prevLeftIdx = miterLeftIdx;
            prevRightIdx = miterRightIdx;
            prevLeft = left;
        }

        // Add last point
        Vector3 lastLeft = Vector3.Cross(tangents[count - 1], up).normalized;
        Vector3 lastLeftPt = pointList[count - 1] + lastLeft * w;
        Vector3 lastRightPt = pointList[count - 1] - lastLeft * w;
        int lastLeftIdx = vertices.Count;
        vertices.Add(lastLeftPt);
        int lastRightIdx = vertices.Count;
        vertices.Add(lastRightPt);
        if (uvExt == UVExtend.ExtendAlongU)
        {
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(0, 0));
        }
        else
        {
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
        }        
        AddQuad(triangles, prevLeftIdx, prevRightIdx, lastLeftIdx, lastRightIdx);

        if(mesh == null)
            mesh = new Mesh();

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return true;
    }

    public static bool LineLineIntersection(Vector3 p1, Vector3 d1, Vector3 p2, Vector3 d2, out Vector3 intersection)
    {
        intersection = Vector3.zero;
        Vector3 cross = Vector3.Cross(d1, d2);
        float sqrLen = cross.sqrMagnitude;
        // Two lines are parallel
        if (sqrLen < 1e-6f)
            return false;

        // 求解 p1 + t*d1 与 p2 + s*d2 的最小距离点（两条线可能不严格相交）
        // 使用标准公式计算最近点，并取中点
        Vector3 p2p1 = p2 - p1;
        float t = Vector3.Dot(Vector3.Cross(p2p1, d2), cross) / sqrLen;
        float s = Vector3.Dot(Vector3.Cross(p2p1, d1), cross) / sqrLen;
        Vector3 pointOnLine1 = p1 + t * d1;
        Vector3 pointOnLine2 = p2 + s * d2;
        intersection = (pointOnLine1 + pointOnLine2) * 0.5f;
        return true;
    }

    private static void AddQuad(List<int> triangles, int l0, int r0, int l1, int r1)
    {
        triangles.Add(l0);
        triangles.Add(l1);
        triangles.Add(r0);

        triangles.Add(l1);
        triangles.Add(r1);
        triangles.Add(r0);
    }    
}
