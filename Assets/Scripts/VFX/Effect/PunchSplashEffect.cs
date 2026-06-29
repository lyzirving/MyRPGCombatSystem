using UnityEngine;
using System.Collections.Generic;
using static LineBuilder;
using UnityEngine.Experimental.GlobalIllumination;

public class PunchSplashEffect : MonoBehaviour
{
    [Header("Tracking")]
    public Transform referencePos;
    [Range(0f, 1f)] public float heightFilter = 0.25f;
    [Range(0f, 90f)] public float angleInterval = 80f;
    public float maxDropHeight = 0.05f;
    public float minimumInterval = 0.08f;
    public bool smoothPoints = true;
    public int smoothWindowSize = 3;
    public float miterLimit = 2f;

    [Header("Renderering")]
    public Material material;
    public float width = 1f;
    public float scaleRatio = 1f;
    public float rotationAngle = 0f;

    [Header("Debug")]
    public float gizmoWidth = 0.05f;

    public bool canMakeMesh => m_Path != null && m_Path.Count >= 2;

    private List<Vector3> m_Path = new List<Vector3>();
    private List<Vector3> m_SmoothedPath = new List<Vector3>();
    private bool m_IsRecording = false;

    private Mesh m_Mesh = null;
    private Vector3 m_AverageForward = Vector3.forward;
    private Vector3 m_AverageLeft = -Vector3.right;
    private float m_FilteredHeight = 0f;
    private float m_LastScaleRatio = 1f;

    private Matrix4x4 m_ScaleMatrix = Matrix4x4.identity;
    private Matrix4x4 m_RotationMatrix = Matrix4x4.identity;

    #region Lifecycle
    private void Awake()
    {
        m_Mesh = new Mesh();
        m_Mesh.MarkDynamic();
    }

    private void Start()
    {
        m_ScaleMatrix = Matrix4x4.identity;
        m_LastScaleRatio = 1f;
        m_AverageForward = Vector3.forward;
        m_AverageLeft = -Vector3.right;
    }

    private void LateUpdate()
    {
        CalcMatrix();
        RecordPath();
        RenderMesh();
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < m_Path.Count; i++)
            Gizmos.DrawSphere(m_Path[i], gizmoWidth);
    }
    #endregion

    #region Core API
    private void RecordPath()
    {
        if (!m_IsRecording)
            return;

        if (referencePos == null)
        {
            Debug.LogWarning("referencePos is null while recording");
            return;
        }

        int count = m_Path.Count;
        if (count == 0)
        {
            m_Path.Add(referencePos.position);
            m_FilteredHeight = referencePos.position.y;
            return;
        }

        Vector3 current = referencePos.position;
        if (!IsDistanceExceed(current, m_Path))
        {
            if (count == 1)
            {
                current.y = CalcFilteredHeight(m_FilteredHeight, current.y);
                m_FilteredHeight = current.y;
                m_Path.Add(current);
                return;
            }

            if (IsAngleExceed(current, m_Path))
                return;

            if (IsHeightDropExceed(current, m_Path))
                current.y = m_Path[count - 1].y;

            current.y = CalcFilteredHeight(m_FilteredHeight, current.y);
            m_FilteredHeight = current.y;
            m_Path.Add(current);
        }
    }

    private void RenderMesh()
    {
        if (m_Mesh == null || m_Mesh.vertexCount == 0)
            return;

        Graphics.DrawMesh(m_Mesh, m_RotationMatrix * m_ScaleMatrix, material, gameObject.layer);
    }

    private bool IsDistanceExceed(Vector3 pt, List<Vector3> points)
    {
        if (points.Count == 0)
            return true;

        return Vector3.Distance(pt, points[points.Count - 1]) < minimumInterval;
    }

    private bool IsAngleExceed(Vector3 pt, List<Vector3> points)
    {
        var count = points.Count;
        if (count < 3)
            return false;
        Vector3 dir0 = (pt - points[count - 1]).normalized;
        Vector3 dir1 = (points[count - 1] - points[count - 2]).normalized;
        return Vector3.Angle(dir0, dir1) > angleInterval;
    }

    private bool IsHeightDropExceed(Vector3 pt, List<Vector3> points)
    {
        var count = points.Count;
        if (count == 0)
            return false;
        float drop = pt.y - points[count - 1].y;
        return drop < 0 && Mathf.Abs(drop) >= maxDropHeight;
    }

    private float CalcFilteredHeight(float filteredHeight, float currentHeight)
    {
        return Mathf.Lerp(filteredHeight, currentHeight, heightFilter);
    }

    private void CalcMatrix()
    {
        CalcScaleMatrix();
    }

    private void CalcScaleMatrix()
    {
        if(m_Mesh.vertexCount == 0)
            return;
        
        if (Mathf.Approximately(scaleRatio, m_LastScaleRatio))
            return;
        
        if (scaleRatio > 0f && scaleRatio < Mathf.Epsilon)
            scaleRatio = 0.01f;
        
        var center = m_Mesh.bounds.center;
        
        m_ScaleMatrix = Matrix4x4.Translate(center) *
                        ScaleAlongAxis(m_AverageForward, scaleRatio) *
                        Matrix4x4.Translate(-center);
        
        m_LastScaleRatio = scaleRatio;
    }

    private void CalcRotateMatrix()
    {
        if (m_Mesh.vertexCount == 0)
            return;

        var center = m_Mesh.bounds.center;
        float angle = Vector3.Angle(Vector3.up, m_AverageLeft);
        m_RotationMatrix = Matrix4x4.Translate(center) *
            Matrix4x4.Rotate(Quaternion.AngleAxis(-angle, m_AverageForward)) *
            Matrix4x4.Translate(-center);
    }

    /// <summary>
    /// Scale along any specific axis at the center of (0, 0, 0)
    /// </summary>
    /// <param name="normalizedAxis"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    private Matrix4x4 ScaleAlongAxis(Vector3 normalizedAxis, float k)
    {
        float x = normalizedAxis.x;
        float y = normalizedAxis.y;
        float z = normalizedAxis.z;
        float k1 = k - 1.0f;

        Matrix4x4 s = Matrix4x4.identity;
        s.m00 = 1.0f + k1 * x * x;
        s.m01 = k1 * x * y;
        s.m02 = k1 * x * z;

        s.m10 = k1 * y * x;
        s.m11 = 1.0f + k1 * y * y;
        s.m12 = k1 * y * z;

        s.m20 = k1 * z * x;
        s.m21 = k1 * z * y;
        s.m22 = 1.0f + k1 * z * z;

        return s;
    }

    private void SmoothPoints(List<Vector3> points, List<Vector3> smoothed, int windowSize = 3)
    {
        if(smoothed == null)
            smoothed = new List<Vector3>();

        smoothed.Clear();

        for (int i = 0; i < points.Count; i++)
        {
            // NOTE：windowSize should be odd number(3, 5, 7 ......),
            //       it can make current point be the center of window.
            int half = windowSize / 2;
            int start = Mathf.Max(0, i - half);
            int end = Mathf.Min(points.Count - 1, i + half);

            Vector3 sum = Vector3.zero;
            int count = 0;
            for (int j = start; j <= end; j++)
            {
                sum += points[j];
                count++;
            }

            Vector3 average = sum / count;
            smoothed.Add(average);
        }
    }

    private bool BuildLineMesh(Vector3 up, UVExtend uvExt)
    {
        int count = m_SmoothedPath == null ? 0 : m_SmoothedPath.Count;
        if (count < 2)
            return false;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        float totalWeight = 0f;
        m_AverageForward = Vector3.zero;
        Vector3[] tangents = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            if (i == 0)
                tangents[i] = m_SmoothedPath[1] - m_SmoothedPath[0];
            else if (i == count - 1)
                tangents[i] = m_SmoothedPath[count - 1] - m_SmoothedPath[count - 2];
            else
                tangents[i] = m_SmoothedPath[i + 1] - m_SmoothedPath[i - 1];

            float len = tangents[i].magnitude;
            totalWeight += len;
            tangents[i] = tangents[i].normalized;
            m_AverageForward += tangents[i] * len;
        }
        m_AverageForward = (m_AverageForward / totalWeight).normalized;
        m_AverageLeft = Vector3.Cross(m_AverageForward, up).normalized;

        Vector3 firstLeft = Vector3.Cross(tangents[0], up).normalized;
        Vector3 firstLeftPt = m_SmoothedPath[0] + firstLeft * width;
        Vector3 firstRightPt = m_SmoothedPath[0] - firstLeft * width;

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
            Vector3 leftPt = m_SmoothedPath[i] + left * width;
            Vector3 rightPt = m_SmoothedPath[i] - left * width;

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

                Vector3 bevelLeftPrev = m_SmoothedPath[i] + prevLeft * width;
                Vector3 bevelRightPrev = m_SmoothedPath[i] - prevLeft * width;

                Vector3 bevelLeftCurr = m_SmoothedPath[i] + left * width;
                Vector3 bevelRightCurr = m_SmoothedPath[i] - left * width;

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
            Vector3 miterLeft = m_SmoothedPath[i] + miterDir * (width * miterScale);
            Vector3 miterRight = m_SmoothedPath[i] - miterDir * (width * miterScale);

            Vector3 interLeft, interRight;
            if (LineLineIntersection(
                m_SmoothedPath[i - 1] + prevLeft * width, prevTangent,
                m_SmoothedPath[i] + left * width, tangent,
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
                m_SmoothedPath[i - 1] - prevLeft * width, prevTangent,
                m_SmoothedPath[i] - left * width, tangent,
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
        Vector3 lastLeftPt = m_SmoothedPath[count - 1] + lastLeft * width;
        Vector3 lastRightPt = m_SmoothedPath[count - 1] - lastLeft * width;
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

        m_Mesh.Clear();
        m_Mesh.SetVertices(vertices);
        m_Mesh.SetUVs(0, uvs);
        m_Mesh.SetTriangles(triangles, 0);
        m_Mesh.RecalculateNormals();
        m_Mesh.RecalculateBounds();

        return true;
    }

    private void AddQuad(List<int> triangles, int l0, int r0, int l1, int r1)
    {
        triangles.Add(l0);
        triangles.Add(l1);
        triangles.Add(r0);

        triangles.Add(l1);
        triangles.Add(r1);
        triangles.Add(r0);
    }

    private bool LineLineIntersection(Vector3 p1, Vector3 d1, Vector3 p2, Vector3 d2, out Vector3 intersection)
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
    #endregion

    #region Public API
    public void StartRecord()
    {
        m_Path.Clear();
        m_IsRecording = true;
    }

    public void StopRecord()
    {
        m_IsRecording = false;
    }

    public void PlayEffect()
    {
        if (!canMakeMesh)
            return;

        if (smoothPoints)
            SmoothPoints(m_Path, m_SmoothedPath, smoothWindowSize);

        if (BuildLineMesh(Vector3.up, UVExtend.ExtendAlongU))
        {
            m_LastScaleRatio = 1f;
            CalcRotateMatrix();        
        }
    }
    #endregion
}
