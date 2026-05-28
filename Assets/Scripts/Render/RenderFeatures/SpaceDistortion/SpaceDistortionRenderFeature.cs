using UnityEngine.Rendering.Universal;

public class SpaceDistortionRenderFeature : ScriptableRendererFeature
{
    private CopyColorRenderPass m_CopyColorRenderPass = null;
    private SpaceDistortionRenderPass m_SpaceDistortionRenderPass = null;

    public override void Create()
    {
        m_CopyColorRenderPass = new CopyColorRenderPass();
        m_CopyColorRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        m_SpaceDistortionRenderPass = new SpaceDistortionRenderPass();
        m_SpaceDistortionRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Note SpaceDistortionRenderPass should be queued after CopyColorRenderPass.
        // In other words, CopyColorRenderPass should be called first.
        renderer.EnqueuePass(m_CopyColorRenderPass);
        renderer.EnqueuePass(m_SpaceDistortionRenderPass);
    }    
}
