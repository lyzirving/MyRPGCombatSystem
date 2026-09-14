using UnityEngine;

public class Grass : MonoBehaviour
{
    public ComputeShader computeShader;
    public Material material;
    public Camera cam;
    public Terrain terrain;

    public float grassSpacing = 0.1f;
    public int resolution = 100;

    [SerializeField, Range(0, 2)] public float jitterStrength;

    private static readonly int
        grassBladesBufferID = Shader.PropertyToID("_GrassBlades"),
        resolutionID = Shader.PropertyToID("_Resolution"),
        grassPosition = Shader.PropertyToID("_GrassPosition"),
        grassSpacingID = Shader.PropertyToID("_GrassSpacing"),
        jitterStrengthID = Shader.PropertyToID("_JitterStrength"),
        heightMapID = Shader.PropertyToID("_HeightMap"),
        detailMapID = Shader.PropertyToID("_DetailMap"),
        terrainPositionID = Shader.PropertyToID("_TerrainPosition"),        
        heightMapScaleID = Shader.PropertyToID("_HeightMapScale"),// transform world position to height map's uv
        heightMapMultiplierID = Shader.PropertyToID("_HeightMapMultiplier"),
        hasTerrainID = Shader.PropertyToID("_HasTerrain");

    private const int ARGS_STRIDE = sizeof(int) * 5;
    private ComputeBuffer m_GrassBladesBuffer;
    private ComputeBuffer m_MeshTrianglesBuffer;
    private ComputeBuffer m_MeshColorsBuffer;
    private ComputeBuffer m_MeshUvsBuffer;
    private ComputeBuffer m_ArgsBuffer;    
    private Mesh m_ClonedMesh;
    private Bounds m_Bounds;

    // Largest resolution the currently allocated append buffer can hold.
    // The buffer only needs to grow when resolution increases beyond this,
    // otherwise appends are silently dropped once the old capacity is reached.
    private int m_AllocatedResolution;

    void Awake()
    {
        Initialize();

        m_Bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
    }

    void Update()
    {
        UpdateGpuParameters();
    }

    void LateUpdate()
    {
        RenderGrass();
    }

    void OnDestroy()
    {
        DisposeBuffers();
    }

    private void Initialize()
    {
        InitializeComputeBuffers();
        SetupMeshBuffers();
    }

    private void InitializeComputeBuffers()
    {
        m_GrassBladesBuffer = new ComputeBuffer(resolution * resolution, sizeof(float) * 3, ComputeBufferType.Append);
        m_GrassBladesBuffer.SetCounterValue(0);
        m_AllocatedResolution = resolution;

        m_ArgsBuffer = new ComputeBuffer(1, ARGS_STRIDE, ComputeBufferType.IndirectArguments);
    }

    private void ReallocateGrassBladesBuffer()
    {
        DisposeBuffer(m_GrassBladesBuffer);

        m_GrassBladesBuffer = new ComputeBuffer(resolution * resolution, sizeof(float) * 3, ComputeBufferType.Append);
        m_GrassBladesBuffer.SetCounterValue(0);
        m_AllocatedResolution = resolution;

        // The material reads _GrassBlades during the indirect draw, so rebind
        // the newly allocated buffer in addition to the per-frame compute bind.
        material.SetBuffer(grassBladesBufferID, m_GrassBladesBuffer);
    }

    private void SetupMeshBuffers()
    {
        m_ClonedMesh = GrassMesh.CreateHighLODMesh();
        m_ClonedMesh.name = "Grass Instance Mesh";

        CreateComputeBuffersForMesh();

        // Initialize args buffer with mesh triangle count. Instance count will be updated later in GPU.
        m_ArgsBuffer.SetData(new int[] { m_MeshTrianglesBuffer.count, 0, 0, 0, 0 });
    }

    private ComputeBuffer CreateBuffer<T>(T[] data, int stride) where T : struct
    {
        ComputeBuffer buffer = new ComputeBuffer(data.Length, stride);
        buffer.SetData(data);
        return buffer;
    }

    private void CreateComputeBuffersForMesh()
    {
        int[] triangles = m_ClonedMesh.triangles;
        Color[] colors = m_ClonedMesh.colors;
        Vector2[] uvs = m_ClonedMesh.uv;

        m_MeshTrianglesBuffer = CreateBuffer<int>(triangles, sizeof(int));
        m_MeshColorsBuffer = CreateBuffer<Color>(colors, sizeof(float) * 4);
        m_MeshUvsBuffer = CreateBuffer<Vector2>(uvs, sizeof(float) * 2);

        material.SetBuffer("Triangles", m_MeshTrianglesBuffer);
        material.SetBuffer("Colors", m_MeshColorsBuffer);
        material.SetBuffer("Uvs", m_MeshUvsBuffer);
        material.SetBuffer(grassBladesBufferID, m_GrassBladesBuffer);
    }

    private void UpdateGpuParameters()
    {
        if (resolution > m_AllocatedResolution)
            ReallocateGrassBladesBuffer();

        m_GrassBladesBuffer.SetCounterValue(0);

        SetupComputeShader();

        int threadGroupsX = Mathf.CeilToInt(resolution / 8f);
        int threadGroupsZ = Mathf.CeilToInt(resolution / 8f);

        computeShader.Dispatch(0, threadGroupsX, threadGroupsZ, 1);
    }

    private void SetupComputeShader()
    {
        computeShader.SetInt(resolutionID, resolution);
        computeShader.SetBuffer(0, grassBladesBufferID, m_GrassBladesBuffer);
        computeShader.SetFloat(grassSpacingID, grassSpacing);
        computeShader.SetFloat(jitterStrengthID, jitterStrength);
        computeShader.SetVector(grassPosition, transform.position);

        if (terrain != null)
        {
            computeShader.SetInt(hasTerrainID, 1);
            computeShader.SetVector(terrainPositionID, terrain.transform.position);
            computeShader.SetTexture(0, heightMapID, terrain.terrainData.heightmapTexture);
            // blending weight
            if (terrain.terrainData.alphamapTextures.Length > 0)
                computeShader.SetTexture(0, detailMapID, terrain.terrainData.alphamapTextures[0]);
            computeShader.SetFloat(heightMapScaleID, terrain.terrainData.size.x);
            computeShader.SetFloat(heightMapMultiplierID, terrain.terrainData.size.y);
        }
        else
        {
            // No terrain assigned: place grass flat on the XZ plane and still
            // bind fallback textures so the kernel doesn't report "not set".
            computeShader.SetInt(hasTerrainID, 0);
            computeShader.SetTexture(0, heightMapID, Texture2D.whiteTexture);
            computeShader.SetTexture(0, detailMapID, Texture2D.whiteTexture);
            computeShader.SetVector(terrainPositionID, Vector3.zero);
            computeShader.SetFloat(heightMapScaleID, 1f);
            computeShader.SetFloat(heightMapMultiplierID, 1f);
        }
    }

    private void RenderGrass()
    {
        ComputeBuffer.CopyCount(m_GrassBladesBuffer, m_ArgsBuffer, sizeof(int)); // Get grass blade count from compute shader.

        // Render grass using procedural indirect draw, utilizing compute buffer for instances.
        Graphics.DrawProceduralIndirect(material, m_Bounds, MeshTopology.Triangles, m_ArgsBuffer,
            0, null, null, UnityEngine.Rendering.ShadowCastingMode.Off, true, gameObject.layer);
    }

    private void DisposeBuffers()
    {
        DisposeBuffer(m_GrassBladesBuffer);
        DisposeBuffer(m_MeshTrianglesBuffer);
        DisposeBuffer(m_MeshColorsBuffer);
        DisposeBuffer(m_MeshUvsBuffer);
        DisposeBuffer(m_ArgsBuffer);
    }

    private void DisposeBuffer(ComputeBuffer buffer)
    {
        buffer?.Dispose();
        buffer = null;
    }
}
