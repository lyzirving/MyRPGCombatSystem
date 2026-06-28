using UnityEngine;
using System.Collections.Generic;
using static LineBuilder;

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

    [Header("Debug")]
    public float gizmoWidth = 0.05f;

    public bool canMakeMesh => m_Path != null && m_Path.Count >= 2;

    private List<Vector3> m_Path = new List<Vector3>();
    private List<Vector3> m_SmoothedPath = new List<Vector3>();
    private bool m_IsRecording = false;

    private Mesh m_Mesh = null;
    private float m_FilteredHeight = 0f;
    private Matrix4x4 m_ScaleMatrix = Matrix4x4.identity;
    private float m_LastScaleRatio = 1f;

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
    }

    private void Update()
    {
        //TODO: how to scale along the path?
        CalcScaleMatrix();
    }    

    private void LateUpdate()
    {
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

        if(referencePos == null)
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

        Graphics.DrawMesh(m_Mesh, m_ScaleMatrix, material, gameObject.layer);
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

    private void CalcScaleMatrix()
    {
        //if(m_Mesh.vertexCount == 0)
        //    return;
        //
        //if (Mathf.Approximately(scaleRatio, m_LastScaleRatio))
        //    return;
        //
        //if (scaleRatio > 0f && scaleRatio < Mathf.Epsilon)
        //    scaleRatio = 0.01f;
        //
        //var center = m_Mesh.bounds.center;
        //
        //m_ScaleMatrix = Matrix4x4.Translate(center) *
        //                ScaleAlongAxis(referenceFwd.forward, scaleRatio) *
        //                Matrix4x4.Translate(-center);
        //
        //m_LastScaleRatio = scaleRatio;
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
            // NOTE£ºwindowSize should be odd number(3, 5, 7 ......),
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

        LineBuilder.BuildLineMesh(m_Path.ToArray(), width, miterLimit, UVExtend.ExtendAlongU, m_Mesh);
    }
    #endregion
}
