using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IGameplayTagSelection
{
    public void OnSelected(GameplayTag tag);
    public void OnCanceled();
}

public class GameplayTagSelectorWindow : EditorWindow
{
    private IGameplayTagSelection m_Callback;    
    private int m_SelectedIndex = -1;
    private Vector2 m_ScrollPosition;

    public static void ShowWindow(IGameplayTagSelection callback)
    {
        // GetWindow is a factory method, and it will create a new instance every time.
        var window = GetWindow<GameplayTagSelectorWindow>("GameplayTag Selection");
        window.Initialize(callback);
        window.minSize = new Vector2(400, 300);
        window.maxSize = new Vector2(800, 600);
        window.Show();
    }

    private void Initialize(IGameplayTagSelection callback)
    {
        GameplayTagManager.instance.LoadDatabase();
        m_Callback = callback;
        m_SelectedIndex = -1;
    }

    private void OnDestroy()
    {
        m_Callback = null;
    }

    private void OnGUI()
    {
        var tags = GameplayTagManager.instance.tags;
        if (tags == null || tags.Count == 0)
        {
            EditorGUILayout.HelpBox("No tags can be selected", MessageType.Info);
            DrawButtons(null);
            return;
        }

        m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

        for (int i = 0; i < tags.Count; ++i)
        {
            bool isSelected = (m_SelectedIndex == i);

            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 16;
            style.margin = new RectOffset(2, 2, 1, 1);            

            if (isSelected)
            {
                Color originalBgColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.3f, 0.5f, 0.8f);
                if (GUILayout.Button(tags[i].name, style, GUILayout.Height(24)))
                {
                    // Do nothing when already being selected
                }
                GUI.backgroundColor = originalBgColor;
            }
            else
            {
                if (GUILayout.Button(tags[i].name, style, GUILayout.Height(24)))
                {
                    m_SelectedIndex = i;
                }
            }
        }

        EditorGUILayout.EndScrollView();

        DrawButtons(tags);
    }

    private void DrawButtons(IReadOnlyList<GameplayTag> tags)
    {
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUI.enabled = (m_SelectedIndex >= 0);
        if (GUILayout.Button("Confirm", GUILayout.Width(100)))
        {
            m_Callback?.OnSelected(tags[m_SelectedIndex]);
            Close();
        }
        GUI.enabled = true;

        if (GUILayout.Button("Cancel", GUILayout.Width(100)))
        {
            m_Callback?.OnCanceled();
            Close();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);
    }
}
