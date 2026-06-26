using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

public class PunchSplashEffect : MonoBehaviour
{
    [Header("Tracking")]
    public Transform referencePos;
    public Transform referenceFwd;

    [Header("Renderering")]
    public Material material;
    public float width = 1f;
    public bool graduallyDecrease = false;
    public float minimumWidthRatio = 0.4f;
    public float minimumInterval = 0.2f;
    public int maxActiveMeshCount = 5;

    [Header("Debug")]
    public float gizmoWidth = 0.05f;

    private ObjectPool<Mesh> m_MeshPool = null;
    private List<Mesh> m_ActiveMesh = new List<Mesh>();
    private List<Vector3> m_Path = new List<Vector3>();
    private bool m_IsRecording = false;

    #region Lifecycle
    private void Awake()
    {
        m_MeshPool = new ObjectPool<Mesh>( 
            createFunc: () => new Mesh(),                      
            actionOnGet: (mesh) => mesh.Clear(),               
            actionOnRelease: (mesh) => mesh.Clear(),           
            actionOnDestroy: (mesh) => mesh.Clear(),                    
            collectionCheck: true,                  
            defaultCapacity: 5,                  
            maxSize: 10                                   
        );
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

        if (referenceFwd == null)
        {
            Debug.LogWarning("referenceFwd is null while recording");
            return;
        }

        if (m_Path.Count == 0)
        {
            m_Path.Add(referencePos.position);
            return;
        }

        int lastIdx = m_Path.Count - 1;
        float interval = Vector3.Distance(referencePos.position, m_Path[lastIdx]);
        if (interval >= minimumInterval)
        {
            var dir = referencePos.position - m_Path[lastIdx];
            var dist = dir.magnitude;
            dir = dir.normalized;

            var angle = Vector3.Angle(referenceFwd.forward, dir);
            if (angle > 90)
                return;

            m_Path.Add(m_Path[lastIdx] + referenceFwd.forward * Vector3.Dot(referenceFwd.forward, dir) * dist);
        }
    }

    private Mesh BuildMesh()
    {
        int pointCount = m_Path.Count;
        if (pointCount < 2)
            return null;

        Vector3[] vertices = new Vector3[pointCount * 2];
        Vector2[] uvs = new Vector2[pointCount * 2];

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 dir;
            if (i == pointCount - 1)
                dir = (m_Path[i] - m_Path[i - 1]).normalized;
            else
                dir = (m_Path[i + 1] - m_Path[i]).normalized;

            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(dir, up)) > 0.99f)
                up = Vector3.forward;
            Vector3 left = Vector3.Cross(dir, up).normalized;
            Vector3 normal = Vector3.Cross(left, dir).normalized;

            // tail: i = 0; head: i = n - 1
            float progress = (float)i / (float)(pointCount - 1);
            float currentWidth = graduallyDecrease ? Mathf.Lerp(width * minimumWidthRatio, width, progress) : width;

            // top && bottom
            vertices[i * 2] = m_Path[i] + normal * currentWidth;
            vertices[i * 2 + 1] = m_Path[i] - normal * currentWidth;

            // UV: U goes along with the path        
            float u = 1f - progress;
            uvs[i * 2] = new Vector2(u, 1);
            uvs[i * 2 + 1] = new Vector2(u, 0);
        }

        int[] triangles = new int[(pointCount - 1) * 6];
        for (int i = 0; i < pointCount - 1; i++)
        {
            int i0 = i * 2;
            int i1 = i * 2 + 1;
            int i2 = (i + 1) * 2;
            int i3 = (i + 1) * 2 + 1;

            // triangle1: 0-1-2
            triangles[i * 6] = i0;
            triangles[i * 6 + 1] = i2;
            triangles[i * 6 + 2] = i1;

            // triangle2: 2-1-3
            triangles[i * 6 + 3] = i2;
            triangles[i * 6 + 4] = i3;
            triangles[i * 6 + 5] = i1;
        }

        var mesh = m_MeshPool.Get();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void RenderMesh()
    {
        foreach (var mesh in m_ActiveMesh)
            Graphics.DrawMesh(mesh, Matrix4x4.identity, material, gameObject.layer);
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
        Mesh mesh = BuildMesh();
        if (mesh != null)
        {
            m_ActiveMesh.Add(mesh);
            if (m_ActiveMesh.Count > maxActiveMeshCount)
            {
                Mesh removed = m_ActiveMesh[0];
                m_ActiveMesh.RemoveAt(0);
                m_MeshPool.Release(removed);
            }
        }
    }
    #endregion
}
