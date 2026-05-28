using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class CopyColorRenderPass : ScriptableRenderPass
{
    private Material m_BlitMaterial;

    private class PassData
    {
        public TextureHandle source;
    }

    public CopyColorRenderPass()
    {
        // Get a group of necessary shaders the URP needs in runtime 
        if (GraphicsSettings.TryGetRenderPipelineSettings<UniversalRenderPipelineRuntimeShaders>(out var settings))
        {
            // Create a material instance using the coreBlitPS shader.
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(settings.coreBlitPS);
        }
        else
        {
            Debug.LogWarning("CopyColorRenderPass: fail to get pipeline settings");
        }
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        // Get camera data
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        // Get resource data, including active color texture
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
        // Pure color template, so we don't depth and stencil
        desc.depthStencilFormat = GraphicsFormat.None;
        // MSAA isn't needed
        desc.msaaSamples = 1;
        desc.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;

        ///////////////////////////////////
        /// RecordRenderGraph is a compile and record stage, it's not the executing stage.
        /// CreateRenderGraphTexture will declare a resource request, and engine won't create resource immediately.
        /// In the real executing stage, the engine will create or retrieve a texture with the same desc in the pool.
        ///////////////////////////////////

        // Create temporary texture handle, and name it as "_CameraColorTexture" for possible further use
        // targetTexture is a lightweight handle, it's a logic resource, not the real gpu resource.
        TextureHandle targetTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_CameraColorTexture", true);

        // Add a raster render pass and name it as "CopyColor"
        using (var builder = renderGraph.AddRasterRenderPass("CopyColor", out PassData passData))
        {
            passData.source = resourceData.activeColorTexture;

            // By default all passes can be culled out if the render graph detects it's not actually used
            builder.AllowPassCulling(false);

            // Attach targetTexture as the output render target
            builder.SetRenderAttachment(targetTexture, 0, AccessFlags.Write);

            // Make activeColorTexture as the input source texture
            builder.UseTexture(passData.source, AccessFlags.Read);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                ///////////////////////////////////
                /// Copy the activeColorTexture every frame will be high performance overhead
                /// Considering use the activeColorTexture directly?
                ///////////////////////////////////

                // use Blitter to copy source texture(resourceData.activeColorTexture) into render target(tempColor)
                Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), m_BlitMaterial, 0);
            });

            // Set _CameraColorTexture as global attribute for shader after current pass's SetRenderFunc is called
            builder.SetGlobalTextureAfterPass(targetTexture, Shader.PropertyToID("_CameraColorTexture"));
        }
    }
}
