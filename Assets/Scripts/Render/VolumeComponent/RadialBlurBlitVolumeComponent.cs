using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

// Add the Volume Override to the list of available Volume Override components in the Volume Profile.
[VolumeComponentMenu("Post-processing Custom/RadialBlurBlitEffect")]
// If the related Scriptable Renderer Feature doesn't exist, display a warning about adding it to the renderer.
[VolumeRequiresRendererFeatures(typeof(RadialBlurBlitFeature))]
// Make the Volume Override active in the Universal Render Pipeline.
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public sealed class RadialBlurBlitVolumeComponent : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Enter the description for the property that is shown when hovered")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public ClampedFloatParameter duration = new ClampedFloatParameter(0.4f, 0.1f, 1f);
    public Vector2Parameter focusCenter = new Vector2Parameter(Vector2.zero);
    public Vector2Parameter moveDirection = new Vector2Parameter(new Vector2(1f, 0f));

    // Set the name of the volume component in the list in the Volume Profile.
    public RadialBlurBlitVolumeComponent()
    {
        displayName = "RadialBlurBlitEffect";
    }

    public void UpdateFocusCenter(Camera camera, Vector3 worldPos, Vector2 moveDirOnScreen)
    {
        if (camera == null) return;

        Vector3 viewport = camera.WorldToViewportPoint(worldPos);
        if (viewport.x >= -0.1f && viewport.x <= 1.1f &&
            viewport.y >= -0.1f && viewport.y <= 1.1f &&
            viewport.z > 0)
        {
            focusCenter.value = viewport;
            moveDirection.value = moveDirOnScreen.normalized;
        }
    }

    public bool IsActive()
    {
        return intensity.GetValue<float>() > 0.0f;
    }
}
