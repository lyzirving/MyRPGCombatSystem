using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameplayTagDatabase))]
public class GameplayTagSettingsEditor : Editor
{
    private MonoScript m_Script = null;
    private GUIStyle m_RichBoxStyle = null;

    private TagEditorNode m_TagRootNode = null;
    private GameplayTagDatabase m_Target = null;
    private bool m_FirstEnter = true;

    private void OnEnable()
    {
        m_Target = target as GameplayTagDatabase;
        m_Script = MonoScript.FromScriptableObject(m_Target);
        m_FirstEnter = true;

        GameplayTagManager.instance.PrepareTagNodeTree(m_Target);
        m_TagRootNode = TagEditorNode.BuildEditorTreeFromDatabase(m_Target);
    }    

    public override void OnInspectorGUI()
    {
        if (EditorApplication.isCompiling)
            return;

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", m_Script, typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gameplay Tags", EditorStyles.boldLabel);        

        if (m_TagRootNode == null)
        {
            EditorGUILayout.HelpBox("No tags defined. Use the button below to add root tags.", MessageType.Info);
            EditorGUILayout.Space();

            if (GUILayout.Button("Add Root Tag"))
            {
                GameplayTagManager.instance.PrepareTagNodeTree(m_Target);
                m_TagRootNode = TagEditorNode.BuildEditorTreeFromDatabase(m_Target);
            }
        }

        if (m_TagRootNode != null)
        {
            if (m_FirstEnter)
            {
                m_FirstEnter = false;
                m_TagRootNode.expand = m_TagRootNode.children.Count != 0;
            }

            bool hierarchyChange = false;
            DrawTagNode(m_TagRootNode, ref hierarchyChange);

            if (hierarchyChange)
                ApplyChangeToTarget();
        }

        DrawSaveButton();
    }    

    private void DrawTagNode(TagEditorNode node, ref bool hierarchyChange)
    {
        if(node == null)
            return;

        EditorGUILayout.BeginHorizontal();

        int space = node.depth - 1;
        if (space > 0)
            GUILayout.Space(space * 10f);

        if (GUILayout.Button(node.expand ? "▼" : "▶", GUILayout.Width(22)))
        {
            node.expand = !node.expand;
        }        

        DrawNodeShortName(node, ref hierarchyChange);               

        // Add button
        if (GUILayout.Button("+", GUILayout.Width(22)))
        {
            InsertChildNode(node);
            node.expand = true;
            hierarchyChange = true;
        }

        if (node.isRoot) EditorGUI.BeginDisabledGroup(true);
        // Delete button
        if (GUILayout.Button("×", GUILayout.Width(22)))
        {
            node.expand = false;
            node.parent.DeleteChild(node);
            hierarchyChange = true;
        }
        if (node.isRoot) EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();

        if (node.expand)
        {
            for (int i = 0; i < node.children.Count; ++i)
                DrawTagNode(node.children[i], ref hierarchyChange);
        }        
    }

    private void DrawNodeShortName(TagEditorNode node, ref bool hierarchyChange)
    {
        CreateRichBoxStyle();

        if (node.isRoot) EditorGUI.BeginDisabledGroup(true);

        EditorGUI.BeginChangeCheck();
        node.shortName = GUILayout.TextField(node.shortName);
        if (EditorGUI.EndChangeCheck())
        {
            node.ApplyShortNameChange();
            hierarchyChange = true;
        }

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

        if (node.isRoot) EditorGUI.EndDisabledGroup();
    }

    private void DrawSaveButton()
    {
        if (!EditorUtility.IsDirty(target)) EditorGUI.BeginDisabledGroup(true);
        if (GUILayout.Button("Apply", GUILayout.Width(60)))
            AssetDatabase.SaveAssetIfDirty(target);
        if (!EditorUtility.IsDirty(target)) EditorGUI.EndDisabledGroup();
    }

    private void InsertChildNode(TagEditorNode node)
    {
        var child = new TagEditorNode();
        child.parent = node;
        child.fullName = new StringBuilder(node.fullName.ToString()).Append(".").ToString();

        node.children.Add(child);
    }    

    private void ApplyChangeToTarget()
    {
        m_Target.allTags.Clear();
        ApplyNodeToTarget(m_TagRootNode, m_Target.allTags);

        EditorUtility.SetDirty(target);
    }

    private void ApplyNodeToTarget(TagEditorNode node, List<GameplayTag> tagList)
    {
        if (node == null || string.IsNullOrEmpty(node.fullName))
            return;

        tagList.Add(new GameplayTag(node.fullName));

        for (int i = 0; i < node.children.Count; ++i)
        {
            var child = node.children[i];
            ApplyNodeToTarget(child, tagList);
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
}