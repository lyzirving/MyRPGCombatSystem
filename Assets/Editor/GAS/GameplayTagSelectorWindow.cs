using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IGameplayTagSelection
{
    public void OnTagSelected(GameplayTag tag);
    public void OnTagCanceled();
}

public class GameplayTagSelectorWindow : EditorWindow
{
    private static readonly Color k_HighlightColor = new Color(0.3f, 0.5f, 0.8f);

    private IGameplayTagSelection m_Callback;

    private GUIStyle m_RichBoxStyle = null;
    private TagEditorNode m_TagRootNode = null;
    private TagEditorNode m_SelectedNode = null;

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
        GameplayTagManager.instance.LoadTagDatabase();
        m_TagRootNode = TagEditorNode.BuildEditorTreeFromTagList(GameplayTagManager.instance.tags);
        m_SelectedNode = null;
        m_Callback = callback;
    }

    private void OnDestroy()
    {
        m_Callback = null;
    }

    private void OnGUI()
    {
        var tags = GameplayTagManager.instance.tags;
        DrawTagTree();
        DrawButtons(tags);
    }

    private void DrawTagTree()
    {
        if (m_TagRootNode == null)
        {
            EditorGUILayout.HelpBox("No tags defined. Please exit and add root node to the asset.", MessageType.Info);
            return;
        }
        m_TagRootNode.expand = m_TagRootNode.children.Count > 0;
        DrawTagNode(m_TagRootNode);
    }

    private void DrawTagNode(TagEditorNode node)
    {
        if (node == null)
            return;

        EditorGUILayout.BeginHorizontal();

        int space = node.depth - 1;
        if (space > 0)
            GUILayout.Space(space * 10f);

        if (GUILayout.Button(node.expand ? "▼" : "▶", GUILayout.Width(22)))
        {
            node.expand = !node.expand;
        }

        DrawNodeShortName(node);

        EditorGUILayout.EndHorizontal();

        if (node.expand)
        {
            for (int i = 0; i < node.children.Count; ++i)
                DrawTagNode(node.children[i]);
        }
    }

    private void DrawNodeShortName(TagEditorNode node)
    {
        bool isSelected = node == m_SelectedNode;
        bool isRootNode = node.isRoot;
        Color originalBgColor = GUI.backgroundColor;

        if (isSelected && !isRootNode)
            GUI.backgroundColor = k_HighlightColor;

        if (GUILayout.Button(node.shortName, "TextField") && !isRootNode)
        {
            m_SelectedNode = node;
        }
        GUI.backgroundColor = originalBgColor;

        CreateRichBoxStyle();
        var lastRect = GUILayoutUtility.GetLastRect();
        bool showTooltip = lastRect.Contains(Event.current.mousePosition);
        if (showTooltip && Event.current.type == EventType.Repaint)
        {
            string text = $"tag: <b>{node.fullName}</b>, depth: {node.depth}";
            float paddingX = 10f;
            float paddingY = 5f;
            var textSize = m_RichBoxStyle.CalcSize(new GUIContent(text));
            var finalSize = new Vector2(textSize.x + m_RichBoxStyle.border.horizontal + paddingX,
                textSize.y + m_RichBoxStyle.border.vertical + paddingY);
            Vector2 mousePos = Event.current.mousePosition;
            Rect tipRect = new Rect(mousePos.x + 15, mousePos.y - 20, finalSize.x, finalSize.y);
            GUI.Box(tipRect, text, m_RichBoxStyle);
        }
    }    

    private void CreateRichBoxStyle()
    {
        if (m_RichBoxStyle == null)
        {
            m_RichBoxStyle = new GUIStyle(GUI.skin.box);
            m_RichBoxStyle.richText = true;
        }
    }

    private void DrawButtons(IReadOnlyList<GameplayTag> tags)
    {
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUI.enabled = m_SelectedNode != null;
        if (GUILayout.Button("Confirm", GUILayout.Width(100)))
        {
            m_Callback?.OnTagSelected(GameplayTagManager.instance.GetTag(m_SelectedNode.fullName));
            Close();
        }
        GUI.enabled = true;

        if (GUILayout.Button("Cancel", GUILayout.Width(100)))
        {
            m_Callback?.OnTagCanceled();
            Close();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);
    }
}
