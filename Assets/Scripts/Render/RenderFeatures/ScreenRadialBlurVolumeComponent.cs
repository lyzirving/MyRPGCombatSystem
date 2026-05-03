using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Add the Volume Override to the list of available Volume Override components in the Volume Profile.
[VolumeComponentMenu("Post-processing Custom/ScreenRadialBlurEffect")]
// If the related Scriptable Renderer Feature doesn't exist, display a warning about adding it to the renderer.
[VolumeRequiresRendererFeatures(typeof(ScreenRadialBlurFeature))]
// Make the Volume Override active in the Universal Render Pipeline.
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public sealed class ScreenRadialBlurVolumeComponent : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Enter the description for the property that is shown when hovered")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

    // Set the name of the volume component in the list in the Volume Profile.
    public ScreenRadialBlurVolumeComponent()
    {
        displayName = "ScreenRadialBlurEffect";
    }    

    public bool IsActive()
    {
        return intensity.GetValue<float>() > 0.0f;
    }
}
