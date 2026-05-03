using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Performs the actual rendering work
/// This class is referenced to Feature created from: Create -> Rendering -> URP Post-processing Effect(Render Feature with Volume)
/// </summary>
public class ScreenRadialBlurPass : ScriptableRenderPass
{
    // Declare a property block to set additional properties for the material.
    private static MaterialPropertyBlock s_SharedPropertyBlock = new MaterialPropertyBlock();

    // Create shader properties in advance, which is more efficient than referencing them by string.
    private static readonly int kBlitScaleBiasPropertyId = Shader.PropertyToID("_BlitScaleBias");

    private ScreenRadialBlurFeature.Settings m_Settings;
    private Material m_Material;
    private AsyncOperationHandle<Shader> m_AsyncOperationHandle;

    static readonly int ScaleId = Shader.PropertyToID("_Scale");
    static readonly int AlphaId = Shader.PropertyToID("_Alpha");
    static readonly int AnchorXId = Shader.PropertyToID("_AnchorX");
    static readonly int AnchorYId = Shader.PropertyToID("_AnchorY");
    static readonly int DenominatorId = Shader.PropertyToID("_Denominator");

    /// <summary>
    /// A container used to pass information into the Render Graph execution function. 
    /// We place the data needed for the pass inside the PassData, which Render Graph 
    /// safely provides to the execution function when the pass runs.
    /// </summary>
    class PassData
    {
        public Material material;
        public ScreenRadialBlurVolumeComponent volume;
        public float scale;
        public float anchorX;
        public float anchorY;
        public float denominator;
    }

    public ScreenRadialBlurPass(ScreenRadialBlurFeature.Settings settings)
    {
        // Receives the settings from the RendererFeature
        m_Settings = settings;
        m_AsyncOperationHandle = Addressables.LoadAssetAsync<Shader>("Shader/RadialBlur");
        var shader = m_AsyncOperationHandle.WaitForCompletion();
        if (shader == null)
            throw new System.Exception("Fail to find shader at: Shader/RadialBlur");
        
        m_Material = new Material(shader);
    }

    private static void ExecutePass(PassData data, RasterGraphContext context)
    {
        // Clear the material properties.
        s_SharedPropertyBlock.Clear();

        // Set the scale and bias so shaders that use Blit.hlsl work correctly.
        s_SharedPropertyBlock.SetVector(kBlitScaleBiasPropertyId, new Vector4(1, 1, 0, 0));
      
        s_SharedPropertyBlock.SetFloat(AlphaId, data.volume.intensity.value);
        s_SharedPropertyBlock.SetFloat(ScaleId, data.scale);
        s_SharedPropertyBlock.SetFloat(AnchorXId, data.anchorX);
        s_SharedPropertyBlock.SetFloat(AnchorYId, data.anchorY);
        s_SharedPropertyBlock.SetFloat(DenominatorId, data.denominator);        

        // Draw to the current render target.
        context.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3, 1, s_SharedPropertyBlock);
    }

    /// <summary>
    /// URP uses Render Graph to organize rendering work. So instead of issuing draw calls immediately, 
    /// a render pass first describes its work through RecordRenderGraph(). 
    /// This allows URP to analyze pass dependencies and automatically apply several optimizations.
    /// It tells URP:
    /// 1. What objects this pass will draw
    /// 2. Which buffers it will write to
    /// 3. What commands should run when the pass is executed
    /// </summary>
    /// <param name="renderGraph"></param>
    /// <param name="frameData"></param>
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (m_Material == null) return;

        ScreenRadialBlurVolumeComponent myVolume = VolumeManager.instance.stack?.GetComponent<ScreenRadialBlurVolumeComponent>();
        if(myVolume == null || !myVolume.IsActive()) return;

        // Information about the current camera 
        var cameraData = frameData.Get<UniversalCameraData>();
        // The render targets and resources currently used by URP, such as the camera color and depth buffers
        var resourceData = frameData.Get<UniversalResourceData>();

        // When RecordRenderGraph() is called, we are only describing the pass,
        // the actual rendering work will happen later when the Render Graph executes the pass.
        // Because of this deferred execution model, the data required during execution must be stored in a structure that can safely be passed to the render function.
        // When we call AddRasterRenderPass<PassData>() the Render Graph system internally creates an instance of PassData and returns it through the out passData parameter.
        // The render pass must then fill this instance with the data needed when the pass executes.
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Screen Radial Blur Pass", out var passData))
        {
            passData.material = m_Material;
            passData.volume = myVolume;
            passData.scale = m_Settings.scale;
            passData.anchorX = m_Settings.pivotX;
            passData.anchorY = m_Settings.pivotY;
            passData.denominator = m_Settings.denominator;

            TextureHandle destination = resourceData.cameraColor;

            // Set the render graph to render to the temporary texture.            
            builder.SetRenderAttachment(destination, 0/*color attachment slot*/, AccessFlags.Write);
            
            // Set the render method.
            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
        }    
    }

    public void Dispose()
    { 
        m_AsyncOperationHandle.Release();
    }
}
