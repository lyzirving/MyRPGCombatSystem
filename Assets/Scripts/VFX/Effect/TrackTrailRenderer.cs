using UnityEngine;
using System.Collections.Generic;

public class TrackTrailRenderer : MonoBehaviour
{
    public Material material = null;
    public Transform ctlPt0 = null;
    public Transform ctlPt1 = null;
    public Transform endPt = null;
    public float width = 0.5f;
    public float duration = 1f;
    public float growSpeed = 10f;
    public float minSampleDistance = 0.05f;   

    private struct TrailPoint
    {
        public Vector3 position;
        public float timeAlive;
    }

    private Vector3 m_LastPosition = Vector3.zero;
    private float m_TrailLength = 0f;
    private List<TrailPoint> m_TrailPoints = new List<TrailPoint>();    
    private Mesh m_Mesh = null;

    private Vector3 m_Translation = Vector3.zero;

    private void Awake()
    {
        m_Mesh = new Mesh();
        m_Mesh.MarkDynamic();
    }

    private void Start()
    {
        Reset();
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (ctlPt0 == null || ctlPt1 == null || endPt == null)
        {
            Debug.LogWarning("control pts haven't been assigned");
            return;
        }

        if (Mathf.Approximately(duration, 0f))
        {
            Debug.LogWarning("duration is 0");
            return;
        }

        bool isMoving = transform.position != m_LastPosition;
        var deltaTime = Time.deltaTime;
        Vector3 deltaPos = transform.position - m_LastPosition;      
        bool buildMesh = UpdateTrailPoints(deltaPos, deltaTime, isMoving);

        if (buildMesh)
        {
            BuildMesh();

            m_Translation = Vector3.zero;
        }

        if (!buildMesh && isMoving)
            m_Translation += deltaPos;

        RenderMesh();

        m_LastPosition = transform.position;
    }    

    private void OnDrawGizmos()
    {
        for (int i = 0; i < m_TrailPoints.Count; i++)
        {
            Gizmos.DrawSphere(m_TrailPoints[i].position, 0.1f);
        }
    }

    private void Reset()
    {
        m_LastPosition = transform.position;
        m_TrailLength = 0f;
        
        m_TrailPoints.Clear();
        m_Mesh?.Clear();

        m_Translation = Vector3.zero;
    }

    private bool UpdateTrailPoints(Vector3 deltaPos, float deltaTime, bool isMoving)
    {
        bool buildMesh = false;
        bool removed = false;              
        for (int i = m_TrailPoints.Count - 1; i >= 0; --i)
        { 
            var pt = m_TrailPoints[i];
            pt.timeAlive = isMoving ? 0 : (pt.timeAlive + deltaTime);
            pt.position += deltaPos;
            // remove one point per frame
            if (!removed && pt.timeAlive >= duration)
            {
                m_TrailPoints.RemoveAt(i);
                removed = true;
                buildMesh = true;
            }
            else
                m_TrailPoints[i] = pt;
        }

        float dist = Vector3.Distance(transform.position, m_LastPosition);
        if (m_TrailLength < 1f && dist >= minSampleDistance)
        {
            var pt = new TrailPoint();
            pt.timeAlive = 0;
            pt.position = Bezier.Sample(m_TrailLength, transform.position, ctlPt0.position, ctlPt1.position, endPt.position);
            m_TrailPoints.Add(pt);
            m_TrailLength += growSpeed * deltaTime;
            m_TrailLength = Mathf.Clamp01(m_TrailLength);
            buildMesh = true;
        }

        if (m_TrailPoints.Count == 0)
            Reset();

        return buildMesh;
    }       

    private void BuildMesh()
    {
        int pointCount = m_TrailPoints.Count;
        if (pointCount < 2)
        {
            m_Mesh.Clear();
            return;
        }

        //Debug.Log("TrackTrailRenderer: BuildMesh");

        Vector3[] vertices = new Vector3[pointCount * 2];
        Vector2[] uvs = new Vector2[pointCount * 2];
        Color[] colors = new Color[pointCount * 2];

        Vector3 previousDir = (m_TrailPoints[1].position - m_TrailPoints[0].position).normalized;
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 dir;
            if (i == pointCount - 1)
                dir = (m_TrailPoints[i].position - m_TrailPoints[i - 1].position).normalized;
            else
                dir = (m_TrailPoints[i + 1].position - m_TrailPoints[i].position).normalized;

            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(dir, up)) > 0.99f)
                up = Vector3.forward;
            Vector3 right = Vector3.Cross(dir, up).normalized;
            Vector3 normal = Vector3.Cross(right, dir).normalized;

            float progress = (float)i / (float)(pointCount - 1);
            float currentWidth = Mathf.Lerp(width, 0f, /*progress*/0f);

            // left && right
            vertices[i * 2] = m_TrailPoints[i].position - normal * currentWidth;
            vertices[i * 2 + 1] = m_TrailPoints[i].position + normal * currentWidth;            

            // UV: U goes along with the path(head = 0, tail = 1)            
            float u = progress;
            uvs[i * 2] = new Vector2(u, 0);
            uvs[i * 2 + 1] = new Vector2(u, 1);

            float alpha = 1f - progress;
            colors[i * 2] = new Color(1, 1, 1, alpha);
            colors[i * 2 + 1] = new Color(1, 1, 1, alpha);

            previousDir = dir;
        }

        int[] triangles = new int[(pointCount - 1) * 6];
        for (int i = 0; i < pointCount - 1; i++)
        {
            int i0 = i * 2;       
            int i1 = i * 2 + 1;   
            int i2 = (i + 1) * 2;
            int i3 = (i + 1) * 2 + 1; 

            // triangle1: 0-2-1
            triangles[i * 6] = i0;
            triangles[i * 6 + 1] = i2;
            triangles[i * 6 + 2] = i1;

            // triangle2: 2-3-1
            triangles[i * 6 + 3] = i2;
            triangles[i * 6 + 4] = i3;
            triangles[i * 6 + 5] = i1;
        }

        m_Mesh.Clear();
        m_Mesh.vertices = vertices;
        m_Mesh.uv = uvs;
        m_Mesh.colors = colors;
        m_Mesh.triangles = triangles;
        m_Mesh.RecalculateNormals();
        m_Mesh.RecalculateBounds();        
    }

    private void RenderMesh()
    {
        if(m_Mesh == null || m_Mesh.vertexCount == 0)
            return;

        Graphics.DrawMesh(m_Mesh, Matrix4x4.TRS(m_Translation, Quaternion.identity, Vector3.one), material, gameObject.layer);
    }
}
