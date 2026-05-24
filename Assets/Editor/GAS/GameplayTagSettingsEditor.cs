using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
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

    private Vector2 m_LastMousePos = Vector2.zero;
    private float m_MouseFloatingTime = 0f;
    private float m_MouseFloatingStartTime = 0f;
    private bool m_MouseFloating = false;

    private void OnEnable()
    {
        m_Target = target as GameplayTagDatabase;
        m_Script = MonoScript.FromScriptableObject(m_Target);
        m_FirstEnter = true;


        
        if (!GameplayTagManager.instance.isLoaded)
            GameplayTagManager.instance.LoadGameplayTags();
        m_TagRootNode = TagEditorNode.GetRootNode();
        m_TagRootNode.children.Clear();
        TagEditorNode.BuildEditorTree(m_TagRootNode);
    }    

    public override void OnInspectorGUI()
    {
        if (EditorApplication.isCompiling)
            return;

        OnRecordMouseFloatingStart();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", m_Script, typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gameplay Tags", EditorStyles.boldLabel);

        if (m_TagRootNode != null)
        {
            if (m_FirstEnter)
            {
                m_FirstEnter = false;
                m_TagRootNode.expand = m_TagRootNode.children.Count != 0;
            }

            DrawTagNode(m_TagRootNode);

            EditorGUILayout.Space();
            DrawSaveButton();
            DrawGenerateCodeButton();
        }
        else
        {
            EditorGUILayout.HelpBox("Editor root node is null!", MessageType.Warning);
        }

        OnRecordMouseFloatingEnd();
        Repaint();
    }    

    private void DrawTagNode(TagEditorNode node)
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

        DrawNodeShortName(node);               

        // Add button
        if (GUILayout.Button("+", GUILayout.Width(22)))
        {
            node.expand = true;
            var child = InsertChildNode(node);
            if(m_Target.AddTag(child.fullName))
                EditorUtility.SetDirty(target);
        }

        if (node.isRoot) EditorGUI.BeginDisabledGroup(true);
        // Delete button
        if (GUILayout.Button("×", GUILayout.Width(22)))
        {
            node.expand = false;
            node.parent.DeleteChild(node);
            if(m_Target.RemoveTag(node.fullName))
                EditorUtility.SetDirty(target);
        }
        if (node.isRoot) EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();

        if (node.expand)
        {
            for (int i = 0; i < node.children.Count; ++i)
                DrawTagNode(node.children[i]);
        }        
    }

    private void DrawNodeShortName(TagEditorNode node)
    {
        CreateRichBoxStyle();

        if (node.isRoot) EditorGUI.BeginDisabledGroup(true);

        EditorGUI.BeginChangeCheck();
        var oldFullName = node.fullName;
        var oldName = node.shortName;
        node.shortName = GUILayout.TextField(oldName).Trim();
        if (EditorGUI.EndChangeCheck())
        {
            node.ApplyShortNameChange();
            if (m_Target.ChangeTagName(oldFullName, node.fullName))
                EditorUtility.SetDirty(target);
        }

        var lastRect = GUILayoutUtility.GetLastRect();
        if (m_MouseFloating && lastRect.Contains(Event.current.mousePosition))
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
        if (GUILayout.Button("Apply", GUILayout.ExpandWidth(true)))
            AssetDatabase.SaveAssetIfDirty(target);
        if (!EditorUtility.IsDirty(target)) EditorGUI.EndDisabledGroup();
    }

    private void DrawGenerateCodeButton()
    {
        if (GUILayout.Button("Generate Code", GUILayout.ExpandWidth(true)))
        {
            if (EditorUtility.DisplayDialog("Confirm to Generate Code", "Are you sure to generate code for GameplayTag?", "Yes", "No"))
            {
                GameplayTagCodeGenerator.GenerateCodeFile();
            }
        }    
    }

    private TagEditorNode InsertChildNode(TagEditorNode node)
    {
        var child = new TagEditorNode();
        child.parent = node;
        child.fullName = new StringBuilder(node.fullName.ToString()).Append(".")
            .Append($"tag{node.children.Count}").ToString().Trim();
        child.shortName = $"tag{node.children.Count}";
        node.children.Add(child);
        return child;
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

    private void OnRecordMouseFloatingStart()
    {
        if (m_LastMousePos == Event.current.mousePosition)
        {
            if (Mathf.Approximately(m_MouseFloatingStartTime, 0f))
                m_MouseFloatingStartTime = (float)EditorApplication.timeSinceStartup;
            else
                m_MouseFloatingTime = (float)EditorApplication.timeSinceStartup - m_MouseFloatingStartTime;
            m_MouseFloating = (m_MouseFloatingTime >= 0.5f);
        }
        else
        {
            m_MouseFloatingTime = 0f;
            m_MouseFloatingStartTime = 0f;
            m_MouseFloating = false;
        }
    }

    private void OnRecordMouseFloatingEnd()
    {
        m_LastMousePos = Event.current.mousePosition;
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