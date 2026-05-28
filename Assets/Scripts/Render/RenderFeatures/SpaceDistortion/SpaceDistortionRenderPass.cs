using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class SpaceDistortionRenderPass : ScriptableRenderPass
{
    public const string LIGHT_MODE = "SpaceDistortion";

    private class PassData
    {
        public RendererListHandle rendererListHandle;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Space Distortion", out var passData))
        {
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            SortingCriteria sortFlags = cameraData.defaultOpaqueSortFlags;
            RenderQueueRange renderQueueRange = RenderQueueRange.transparent;
            FilteringSettings filterSettings = new FilteringSettings(renderQueueRange, ~0);

            // Only "SpaceDistortion" light mode will be rendered
            ShaderTagId shaderTagId = new ShaderTagId(LIGHT_MODE);

            DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(shaderTagId, renderingData, cameraData, lightData, sortFlags);

            ///////////////////////////
            ///// Shader with light mode "SpaceDistortion" will sample _CameraColorTexture using uv disturbance,
            ////  and draw them onto current active texture
            //////////////////////////

            // Create renderer list with range setting listed as before.            
            var rendererListParameters = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);

            passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParameters);

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            builder.UseRendererList(passData.rendererListHandle);
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
            builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                context.cmd.DrawRendererList(data.rendererListHandle);
            });
        }
    }
}
