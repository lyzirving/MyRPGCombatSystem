using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool that rebinds an orphan animation curve (e.g. "footstep")
/// onto a component field so the curve value can be read at runtime.
/// Drag a .anim (or an FBX-embedded clip) directly.
/// </summary>
public class RebindAnimationCurve : EditorWindow
{
    [MenuItem("Tools/Animation/Rebind Animation Curve")]
    public static void Open() => GetWindow<RebindAnimationCurve>("Rebind Animation Curve");

    private AnimationClip m_Clip;
    private readonly List<EditorCurveBinding> m_CurveBindings = new List<EditorCurveBinding>();
    private string[] m_CurveNames = new string[0];
    private int m_CurveIndex = -1;

    private MonoScript m_TargetScript;
    private string m_RelativePath = "";
    private string m_PropertyName = "";

    private Vector2 m_Scroll;

    private void OnGUI()
    {
        m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
        DrawClipField();
        DrawCurveField();
        DrawTargetFields();
        DrawActions();
        EditorGUILayout.EndScrollView();
    }

    private void DrawClipField()
    {
        EditorGUI.BeginChangeCheck();
        var clip = EditorGUILayout.ObjectField("Clip", m_Clip, typeof(AnimationClip), false) as AnimationClip;
        if (EditorGUI.EndChangeCheck())
        {
            m_Clip = clip;
            RefreshCurves();
        }

        if (m_Clip != null && GUILayout.Button("Copy clip as standalone .anim"))
            CopyClipAsAnim();
    }

    private void DrawCurveField()
    {
        if (m_CurveBindings.Count == 0)
        {
            EditorGUILayout.LabelField("Curve", "No curves");
            return;
        }

        EditorGUI.BeginChangeCheck();
        int idx = EditorGUILayout.Popup("Curve", m_CurveIndex, m_CurveNames);
        if (EditorGUI.EndChangeCheck())
            SelectCurve(idx);
    }

    private void DrawTargetFields()
    {
        m_TargetScript = EditorGUILayout.ObjectField("Component", m_TargetScript, typeof(MonoScript), false) as MonoScript;
        m_RelativePath = EditorGUILayout.TextField("Relative Path", m_RelativePath);
        m_PropertyName = EditorGUILayout.TextField("Property Name", m_PropertyName);

        EditorGUILayout.HelpBox(
            "Relative Path is the path from the Animator's GameObject to the target component's GameObject. Use an empty string when they are on the same object.",
            MessageType.Info);
    }

    private void DrawActions()
    {
        using (new EditorGUI.DisabledGroupScope(!CanRebind()))
        {
            if (GUILayout.Button("Rebind", GUILayout.Height(30)))
                Rebind();
        }
    }

    private void RefreshCurves()
    {
        m_CurveBindings.Clear();
        m_CurveIndex = -1;
        m_PropertyName = "";

        if (m_Clip == null)
        {
            m_CurveNames = new string[0];
            return;
        }

        m_CurveBindings.AddRange(UnityEditor.AnimationUtility.GetCurveBindings(m_Clip));

        m_CurveNames = new string[m_CurveBindings.Count];
        for (int i = 0; i < m_CurveBindings.Count; i++)
        {
            var b = m_CurveBindings[i];
            m_CurveNames[i] = $"{b.propertyName}  (path: {b.path}, type: {(b.type != null ? b.type.Name : "?")})";
        }

        if (m_CurveBindings.Count > 0)
            SelectCurve(0);
    }

    private void SelectCurve(int index)
    {
        m_CurveIndex = index;
        m_PropertyName = m_CurveBindings[index].propertyName;
    }

    private bool CanRebind()
    {
        if (m_Clip == null) return false;
        if (m_CurveIndex < 0 || m_CurveIndex >= m_CurveBindings.Count) return false;
        if (m_TargetScript == null) return false;

        var type = m_TargetScript.GetClass();
        if (type == null || !typeof(Component).IsAssignableFrom(type)) return false;
        if (string.IsNullOrEmpty(m_PropertyName)) return false;

        return true;
    }

    private void Rebind()
    {
        var binding = m_CurveBindings[m_CurveIndex];
        var curve = UnityEditor.AnimationUtility.GetEditorCurve(m_Clip, binding);
        if (curve == null)
        {
            EditorUtility.DisplayDialog("Rebind Animation Curve", "Failed to read the source curve.", "OK");
            return;
        }

        var type = m_TargetScript.GetClass();
        m_Clip.SetCurve(m_RelativePath, type, m_PropertyName, curve);
        EditorUtility.SetDirty(m_Clip);
        AssetDatabase.SaveAssets();

        if (AssetDatabase.IsSubAsset(m_Clip))
        {
            EditorUtility.DisplayDialog("Rebind Animation Curve",
                "Bound successfully.\n\nNOTE: this clip is embedded in an FBX, so the binding will be lost when the FBX reimports. Use \"Copy clip as standalone .anim\" and rebind on the copy to persist it.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Rebind Animation Curve",
                $"Bound \"{m_PropertyName}\" to {type.Name} at path \"{m_RelativePath}\".",
                "OK");
        }
    }

    private void CopyClipAsAnim()
    {
        if (m_Clip == null)
            return;

        var dest = EditorUtility.SaveFilePanelInProject("Save clip as", m_Clip.name + ".anim", "anim", "Save clip as a standalone .anim");
        if (string.IsNullOrEmpty(dest))
            return;

        var copy = new AnimationClip();
        EditorUtility.CopySerialized(m_Clip, copy);
        AssetDatabase.CreateAsset(copy, dest);
        AssetDatabase.SaveAssets();

        m_Clip = copy;
        RefreshCurves();

        EditorUtility.DisplayDialog("Rebind Animation Curve",
            "Copied to:\n" + dest + "\n\nNow selected. Pick the curve and click Rebind.",
            "OK");
    }
}
