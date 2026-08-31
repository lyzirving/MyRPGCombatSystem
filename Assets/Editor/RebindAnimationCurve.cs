using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool that rebinds an FBX-imported orphan animation curve (e.g. "footstep")
/// onto a component field so the curve value can be read at runtime.
/// </summary>
public class RebindAnimationCurve : EditorWindow
{
    [MenuItem("Tools/Animation/Rebind Animation Curve")]
    public static void Open() => GetWindow<RebindAnimationCurve>("Rebind Animation Curve");

    private GameObject m_Fbx;
    private string m_FbxPath = "";

    private readonly List<AnimationClip> m_Clips = new List<AnimationClip>();
    private string[] m_ClipNames = new string[0];
    private int m_ClipIndex = -1;

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
        DrawFbxField();
        DrawClipField();
        DrawCurveField();
        DrawTargetFields();
        DrawActions();
        EditorGUILayout.EndScrollView();
    }

    private void DrawFbxField()
    {
        EditorGUI.BeginChangeCheck();
        var fbx = EditorGUILayout.ObjectField("FBX", m_Fbx, typeof(GameObject), false) as GameObject;
        if (EditorGUI.EndChangeCheck())
        {
            m_Fbx = fbx;
            m_FbxPath = fbx != null ? AssetDatabase.GetAssetPath(fbx) : "";
            RefreshClips();
        }

        using (new EditorGUI.DisabledGroupScope(true))
            EditorGUILayout.TextField("Asset Path", m_FbxPath);
    }

    private void DrawClipField()
    {
        if (m_Clips.Count == 0)
        {
            EditorGUILayout.LabelField("Clip", "No clips loaded");
            return;
        }

        EditorGUI.BeginChangeCheck();
        int idx = EditorGUILayout.Popup("Clip", m_ClipIndex, m_ClipNames);
        if (EditorGUI.EndChangeCheck())
            SelectClip(idx);

        if (GUILayout.Button("Copy clip as standalone .anim"))
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

    private void RefreshClips()
    {
        m_Clips.Clear();
        m_ClipIndex = -1;
        m_CurveBindings.Clear();
        m_CurveIndex = -1;

        if (string.IsNullOrEmpty(m_FbxPath))
        {
            m_ClipNames = new string[0];
            return;
        }

        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(m_FbxPath))
        {
            if (asset is AnimationClip clip)
                m_Clips.Add(clip);
        }

        m_ClipNames = new string[m_Clips.Count];
        for (int i = 0; i < m_Clips.Count; i++)
            m_ClipNames[i] = m_Clips[i].name;

        if (m_Clips.Count > 0)
            SelectClip(0);
    }

    private void SelectClip(int index)
    {
        m_ClipIndex = index;
        m_CurveBindings.Clear();
        m_CurveIndex = -1;

        m_CurveBindings.AddRange(UnityEditor.AnimationUtility.GetCurveBindings(m_Clips[index]));

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
        // Default the target property name to the source curve's name (e.g. "footstep").
        m_PropertyName = m_CurveBindings[index].propertyName;
    }

    private bool CanRebind()
    {
        if (m_ClipIndex < 0 || m_ClipIndex >= m_Clips.Count) return false;
        if (m_CurveIndex < 0 || m_CurveIndex >= m_CurveBindings.Count) return false;
        if (m_TargetScript == null) return false;

        var type = m_TargetScript.GetClass();
        if (type == null || !typeof(Component).IsAssignableFrom(type)) return false;
        if (string.IsNullOrEmpty(m_PropertyName)) return false;

        return true;
    }

    private void Rebind()
    {
        var clip = m_Clips[m_ClipIndex];
        var binding = m_CurveBindings[m_CurveIndex];
        var curve = UnityEditor.AnimationUtility.GetEditorCurve(clip, binding);
        if (curve == null)
        {
            EditorUtility.DisplayDialog("Rebind Footstep", "Failed to read the source curve.", "OK");
            return;
        }

        var type = m_TargetScript.GetClass();
        clip.SetCurve(m_RelativePath, type, m_PropertyName, curve);
        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();

        if (AssetDatabase.IsSubAsset(clip))
        {
            EditorUtility.DisplayDialog("Rebind Footstep",
                "Bound successfully.\n\nNOTE: this clip is embedded in an FBX, so the binding will be lost when the FBX reimports. Use \"Copy clip as standalone .anim\" and rebind on the copy to persist it.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Rebind Footstep",
                $"Bound \"{m_PropertyName}\" to {type.Name} at path \"{m_RelativePath}\".",
                "OK");
        }
    }

    private void CopyClipAsAnim()
    {
        if (m_ClipIndex < 0 || m_ClipIndex >= m_Clips.Count)
            return;

        var source = m_Clips[m_ClipIndex];
        var dest = EditorUtility.SaveFilePanelInProject("Save clip as", source.name + ".anim", "anim", "Save clip as a standalone .anim");
        if (string.IsNullOrEmpty(dest))
            return;

        var copy = new AnimationClip();
        EditorUtility.CopySerialized(source, copy);
        AssetDatabase.CreateAsset(copy, dest);
        AssetDatabase.SaveAssets();

        // Reload the standalone .anim so the user can rebind on it directly.
        m_Fbx = null;
        m_FbxPath = dest;
        RefreshClips();

        EditorUtility.DisplayDialog("Rebind Footstep",
            "Copied to:\n" + dest + "\n\nNow selected. Pick the curve and click Rebind.",
            "OK");
    }
}
