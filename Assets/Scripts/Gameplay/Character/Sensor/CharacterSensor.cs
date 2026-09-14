using UnityEngine;

public class CharacterSensor : MonoBehaviour 
{
    [SerializeField] private DistanceZone m_DistanceZone = new DistanceZone();
    [SerializeField] private GroundChecker m_GroundChecker = new GroundChecker();
    private VelocityCache m_VelocityCache;
    private ICharacterBehavior m_CharacterBehavior;    

    /// <summary>
    /// Whether character is standing on a collidable surface (any collider, layer-agnostic)
    /// </summary>
    public bool isGrounded => m_GroundChecker.isGrounded;
    /// <summary>
    /// Character's average speed on ground
    /// </summary>
    public Vector3 averageVelocity => m_VelocityCache.averageVelocity;
    public DistanceZone distZone => m_DistanceZone;

    #region Main Methods
    public void Init(ICharacterBehavior behavior)
    {
        m_CharacterBehavior = behavior;

        m_VelocityCache = new VelocityCache(GetComponent<Rigidbody>());

        if (m_GroundChecker == null)
            m_GroundChecker = new GroundChecker();
        m_GroundChecker.onTouch += m_CharacterBehavior.OnContactGround;
        m_GroundChecker.onExit += m_CharacterBehavior.OnExitGround;
        // Probe against every layer; the checker skips this character's own colliders.
        m_GroundChecker.Init(GetComponent<Rigidbody>(), GetComponent<CapsuleCollider>(), GameConsts.Layer.All);

        m_DistanceZone.host = transform;
    }
    #endregion

    #region State Methods
    private void Update()
    {
        m_DistanceZone.UpdateDistance();
    }

    private void FixedUpdate()
    {
        // Probe for ground, snap the character onto it and fire onTouch/onExit.
        m_GroundChecker.Tick();

        if (m_GroundChecker.isGrounded)
            m_VelocityCache.UpdateVelocity();
    }
    #endregion
    
    #region For Gizmos
    private static Material s_LineMaterial;

    private void OnGUI()
    {
        // OnGUI fires for several event types; only draw during the repaint pass.
        if (Event.current.type != EventType.Repaint)
            return;

        if (m_GroundChecker == null || !m_GroundChecker.debugDrawProbe)
            return;

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        Camera cam = Camera.main;
        if (capsule == null || cam == null)
            return;

        Vector3 center = capsule.bounds.center;
        float halfHeight = capsule.height * 0.5f - capsule.radius;
        float radius = capsule.radius;
        float maxDistance = halfHeight + m_GroundChecker.GROUND_PROBE_DISTANCE;

        Vector3 start = center;
        Vector3 end = center + Vector3.down * maxDistance;

        if (!EnsureLineMaterial())
            return;

        GL.PushMatrix();
        GL.LoadPixelMatrix();
        s_LineMaterial.SetPass(0);
        GL.Begin(GL.LINES);

        // Start sphere rings (green) and end sphere rings (yellow).
        DrawWorldCircle(start, radius, Vector3.up, new Color(0f, 1f, 0f, 0.8f), cam);
        DrawWorldCircle(start, radius, Vector3.right, new Color(0f, 1f, 0f, 0.4f), cam);
        DrawWorldCircle(end, radius, Vector3.up, new Color(1f, 1f, 0f, 0.8f), cam);
        DrawWorldCircle(end, radius, Vector3.right, new Color(1f, 1f, 0f, 0.4f), cam);

        // Swept capsule rails + centre line.
        DrawWorldLine(start, end, new Color(1f, 1f, 1f, 0.5f), cam);
        Vector3[] offsets = { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        for (int i = 0; i < offsets.Length; i++)
            DrawWorldLine(start + offsets[i] * radius, end + offsets[i] * radius, new Color(1f, 1f, 1f, 0.4f), cam);

        // Hit point + normal (green when walkable/grounded, red otherwise).
        if (m_GroundChecker.debugHasHit)
        {
            Color hitColor = m_GroundChecker.isGrounded ? Color.green : Color.red;
            DrawWorldCircle(m_GroundChecker.debugHitPoint, 0.06f, Vector3.up, hitColor, cam, 16);
            DrawWorldLine(m_GroundChecker.debugHitPoint, m_GroundChecker.debugHitPoint + m_GroundChecker.debugHitNormal * 0.5f, hitColor, cam);
        }

        GL.End();
        GL.PopMatrix();
    }

    // NOTE: must be called between GL.Begin(GL.LINES) and GL.End().
    private void DrawWorldLine(Vector3 a, Vector3 b, Color color, Camera cam)
    {
        Vector3 sa = cam.WorldToScreenPoint(a);
        Vector3 sb = cam.WorldToScreenPoint(b);
        if (sa.z < 0f || sb.z < 0f)
            return;

        GL.Color(color);
        GL.Vertex3(sa.x, Screen.height - sa.y, 0f);
        GL.Vertex3(sb.x, Screen.height - sb.y, 0f);
    }

    private void DrawWorldCircle(Vector3 center, float radius, Vector3 normal, Color color, Camera cam, int segments = 24)
    {
        Vector3 tangent = Vector3.Cross(normal, Vector3.up);
        if (tangent.sqrMagnitude < 0.0001f)
            tangent = Vector3.Cross(normal, Vector3.forward);
        tangent.Normalize();
        Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

        Vector3 previous = center + tangent * radius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 point = center + (tangent * Mathf.Cos(angle) + bitangent * Mathf.Sin(angle)) * radius;
            DrawWorldLine(previous, point, color, cam);
            previous = point;
        }
    }

    private bool EnsureLineMaterial()
    {
        if (s_LineMaterial != null)
            return true;

        Shader shader = Shader.Find("Hidden/Internal-Colored");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");
        if (shader == null)
            return false;

        s_LineMaterial = new Material(shader);
        s_LineMaterial.hideFlags = HideFlags.HideAndDontSave;
        s_LineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        s_LineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        s_LineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        s_LineMaterial.SetInt("_ZWrite", 0);
        return true;
    }
    #endregion    
}
