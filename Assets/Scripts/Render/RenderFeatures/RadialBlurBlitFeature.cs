using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class RadialBlurBlitFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        [Range(0f, 1f)] public float focusRadius = 0.25f;
        [Range(0f, 0.5f)] public float fade = 0.1f;
        [Range(0f, 1f)] public float blurStrength = 1f;
        public float blurAmount = 5f;
        public int sampleCount = 16;        
        public float directionalBias = 0.3f;
    }

    public Settings settings = new Settings();

    private RadialBlurBlitPass m_BlitPass;

    public override void Create()
    {
        m_BlitPass = new RadialBlurBlitPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_BlitPass == null)
            return;

        // Skip rendering if the camera is outside the custom volume.
        RadialBlurBlitVolumeComponent myVolume = VolumeManager.instance.stack?.GetComponent<RadialBlurBlitVolumeComponent>();
        if (myVolume == null || !myVolume.IsActive())
            return;

        if (m_BlitPass != null)
            renderer.EnqueuePass(m_BlitPass);
    }
}

public class RadialBlurBlitPass : ScriptableRenderPass
{
    private const string SHADER_PATH = "Shader/RadialBlurBlit";
    private RadialBlurBlitFeature.Settings m_Settings;
    private Material m_Material;

    private static readonly int FocusCenterId = Shader.PropertyToID("_FocusCenter");
    private static readonly int FocusRadiusId = Shader.PropertyToID("_FocusRadius");
    private static readonly int FadeId = Shader.PropertyToID("_Fade");
    private static readonly int BlurStrengthId = Shader.PropertyToID("_BlurStrength");
    private static readonly int SampleCountId = Shader.PropertyToID("_SampleCount");
    private static readonly int BlurAmountId = Shader.PropertyToID("_BlurAmount");
    private static readonly int MoveDirectionId = Shader.PropertyToID("_MovingDirection");
    private static readonly int DirectionalBiasId = Shader.PropertyToID("_DirectionalBias");

    public RadialBlurBlitPass(RadialBlurBlitFeature.Settings settings)
    {
        m_Settings = settings;
        var handle = Addressables.LoadAssetAsync<Shader>(SHADER_PATH);
        var shader = handle.WaitForCompletion();
        if (shader == null)
            throw new System.Exception($"Fail to find shader at: {SHADER_PATH}");

        m_Material = new Material(shader);
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

        RadialBlurBlitVolumeComponent myVolume = VolumeManager.instance.stack?.GetComponent<RadialBlurBlitVolumeComponent>();
        if (myVolume == null || !myVolume.IsActive()) return;

        var resourceData = frameData.Get<UniversalResourceData>();

        if (resourceData.isActiveTargetBackBuffer)
        {
            Debug.LogError($"RadialBlurWithFocusFeature: Skipping render pass. The pass requires an intermediate ColorTexture, we can't use the BackBuffer as a texture input.");
            return;
        }

        if (!RenderGraphUtils.CanAddCopyPassMSAA())
        {
            Debug.Log("RadialBlurWithFocusFeature: can't add the copy pass due to MSAA");
            return;
        }

        // The destination texture is created here, 
        // the texture is created with the same dimensions as the active color texture
        var source = resourceData.activeColorTexture;
        var destinationDesc = renderGraph.GetTextureDesc(source);
        destinationDesc.name = "RadialBlur-BlitWithMaterial";
        destinationDesc.clearBuffer = false;      
        TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

        // Blit with material           
        m_Material.SetVector(FocusCenterId, myVolume.focusCenter.value);
        m_Material.SetVector(MoveDirectionId, myVolume.moveDirection.value);

        m_Material.SetInt(SampleCountId, m_Settings.sampleCount);
        m_Material.SetFloat(FocusRadiusId, m_Settings.focusRadius);
        m_Material.SetFloat(FadeId, m_Settings.fade);
        m_Material.SetFloat(BlurStrengthId, m_Settings.blurStrength * myVolume.intensity.value);        
        m_Material.SetFloat(BlurAmountId, m_Settings.blurAmount);        
        m_Material.SetFloat(DirectionalBiasId, m_Settings.directionalBias);

        RenderGraphUtils.BlitMaterialParameters para = new(source, destination, m_Material, 0);
        renderGraph.AddBlitPass(para, "RadialBlur-BlitWithMaterial");
        renderGraph.AddCopyPass(destination, source, "RadialBlur-CopyBack");
    }
}