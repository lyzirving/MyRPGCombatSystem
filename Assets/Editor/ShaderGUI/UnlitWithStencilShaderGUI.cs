using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class UnlitWithStencilShaderGUI : ShaderGUI
{
    private enum SurfaceType
    {
        Opaque = 0,
        Transparent = 1
    }

    private static readonly GUIContent SurfaceTypeContent = new GUIContent("Surface Type", "Choose Surface Type");

    private MaterialProperty m_SurfaceTypeProp;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);

        FindExtraProperties(properties);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Surface Settings", EditorStyles.boldLabel);

        if (DrawSurfaceTypeSelector())
        {
            SurfaceType surface = (SurfaceType)m_SurfaceTypeProp.floatValue;
            ApplySurfaceSettings(materialEditor.target as Material, surface);
        }
    }

    private void FindExtraProperties(MaterialProperty[] properties)
    {
        m_SurfaceTypeProp = FindProperty("_SurfaceType", properties);
    }

    private bool DrawSurfaceTypeSelector()
    {
        EditorGUI.showMixedValue = m_SurfaceTypeProp.hasMixedValue;
        var surfaceType = (SurfaceType)m_SurfaceTypeProp.floatValue;

        EditorGUI.BeginChangeCheck();
        surfaceType = (SurfaceType)EditorGUILayout.EnumPopup(SurfaceTypeContent, surfaceType);

        bool changeCheck = EditorGUI.EndChangeCheck();
        if (changeCheck)
        {
            m_SurfaceTypeProp.floatValue = (float)surfaceType;
        }
        EditorGUI.showMixedValue = false;

        return changeCheck;
    }

    private void ApplySurfaceSettings(Material material, SurfaceType surface)
    {
        switch (surface)
        {
            case SurfaceType.Opaque:
                ApplyOpaqueSetting(material);
                break;
            case SurfaceType.Transparent:
                ApplyTransparentSetting(material);
                break;
            default:
                break;
        }
    }

    private void ApplyOpaqueSetting(Material material)
    {
        Debug.Log($"ApplyOpaqueSetting");

        material.SetInt("_SrcBlend", (int)BlendMode.One);
        material.SetInt("_DstBlend", (int)BlendMode.Zero);
        material.SetInt("_ZWrite", 1);

        material.SetOverrideTag("RenderType", "Opaque");
        material.renderQueue = (int)RenderQueue.Geometry;

        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHA_ON");
    }

    private void ApplyTransparentSetting(Material material)
    {
        Debug.Log($"ApplyTransparentSetting");

        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);

        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = (int)RenderQueue.Transparent;

        material.EnableKeyword("_ALPHA_ON");
    }
}
