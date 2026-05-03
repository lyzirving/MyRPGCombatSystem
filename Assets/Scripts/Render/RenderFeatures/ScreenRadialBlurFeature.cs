using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// RendererFeature's responsibility:
/// 1. Exposes setting
/// 2. Creates the pass
/// 3. Injects it into the renderer
/// </summary>
public class ScreenRadialBlurFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public LayerMask layerMask;
        [Range(0f, 1f)] public float scale = 1f;
        [Range(0f, 1f)] public float intensity = 1f;
        [Range(0f, 1f)] public float pivotX = 0.5f;
        [Range(0f, 1f)] public float pivotY = 0.5f;
        public float denominator = 90f;      
    }

    public Settings settings = new Settings();

    private ScreenRadialBlurPass m_Pass = null;

    /// <summary>
    /// Create() runs when the renderer initializes and instantiates the render pass.
    /// </summary>
    public override void Create()
    {
        m_Pass = new ScreenRadialBlurPass(settings);
    }

    /// <summary>
    /// AddRenderPasses() is called every frame when URP builds the rendering pipeline. 
    /// This is where a Renderer Feature injects its custom stage.
    /// </summary>
    /// <param name="renderer">the active renderer responsible for constructing the frame</param>
    /// <param name="renderingData">contains contextual information for the current frame and camera</param>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Skip rendering if pass instance are null.
        if (m_Pass == null)
        {
            Debug.LogError("ScreenRadialBlurFeature: pass instance is null");
            return;
        }

        // Skip rendering if the target is a Reflection Probe or a preview camera.
        if (renderingData.cameraData.cameraType == CameraType.Preview || 
            renderingData.cameraData.cameraType == CameraType.Reflection)
            return;

        // Skip rendering if the camera is outside the custom volume.
        ScreenRadialBlurVolumeComponent myVolume = VolumeManager.instance.stack?.GetComponent<ScreenRadialBlurVolumeComponent>();
        if (myVolume == null || !myVolume.IsActive())
            return;

        m_Pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        // Specify that the effect doesn't need scene depth, normals, motion vectors,
        // or the color texture as input.
        m_Pass.ConfigureInput(ScriptableRenderPassInput.None);

        renderer.EnqueuePass(m_Pass);
    }

    protected override void Dispose(bool disposing)
    {
        m_Pass?.Dispose();
    }
}
