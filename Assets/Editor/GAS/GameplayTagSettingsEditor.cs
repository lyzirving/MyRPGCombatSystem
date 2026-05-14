using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameplayTagDatabase))]
public class GameplayTagSettingsEditor : Editor
{
    private GUIStyle m_RichBoxStyle = null;

    private TagEditorNode m_TagRootNode = null;
    private GameplayTagDatabase m_Target = null;

    private void OnEnable()
    {
        m_Target = target as GameplayTagDatabase;

        CheckTargetTagList();
        BuildEditorTreeFromTarget();
    }    

    public override void OnInspectorGUI()
    {
        if (EditorApplication.isCompiling)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gameplay Tags", EditorStyles.boldLabel);        

        if (m_TagRootNode == null)
        {
            EditorGUILayout.HelpBox("No tags defined. Use the button below to add root tags.", MessageType.Info);
            EditorGUILayout.Space();

            if (GUILayout.Button("Add Root Tag"))
            {
                CheckTargetTagList();
                BuildEditorTreeFromTarget();
            }
        }

        if (m_TagRootNode != null)
        {
            bool hierarchyChange = false;
            DrawTagNode(m_TagRootNode, ref hierarchyChange);

            if (hierarchyChange)
                ApplyChangeToTarget();
        }      

        Repaint(); 
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

    private void InsertChildNode(TagEditorNode node)
    {
        var child = new TagEditorNode();
        child.parent = node;
        child.fullName = new StringBuilder(node.fullName.ToString()).Append(".").ToString();

        node.children.Add(child);
    }

    private void BuildEditorTreeFromTarget()
    {
        if (m_Target.allTags.Count > 0 && m_Target.allTags[0].isValid)
        {
            var rootTag = m_Target.allTags[0];
            m_TagRootNode = new TagEditorNode();
            BuildEditorTree(m_TagRootNode, null, ref rootTag);
        }
        else
            Debug.Log($"GameplayTagSettingsEditor: tag count[{m_Target.allTags.Count}] is invalid or first tag is invalid");
    }

    private void BuildEditorTree(TagEditorNode node, TagEditorNode parent, ref GameplayTag tag)
    {
        node.parent = parent;
        node.fullName = tag.name;
        node.MakeShortName(tag.name);

        var childTagList = GameplayTagManager.instance.RequestDirectChildren(tag);
        if(childTagList == null || childTagList.Length <= 0)
            return;

        for (int i = 0; i < childTagList.Length; ++i)
        { 
            var childTag = childTagList[i];
            var childNode = new TagEditorNode();

            BuildEditorTree(childNode, node, ref childTag);

            node.children.Add(childNode);
        }
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

    private void CheckTargetTagList()
    {
        if (m_Target.allTags.Count == 0)
        {
            Debug.Log("GameplayTagSettingsEditor: add root tag");
            m_Target.allTags.Add(GameplayTag.RootTag);
        }
        else if (m_Target.allTags.Count > 0 && !m_Target.allTags[0].isValid)
        {
            Debug.Log("GameplayTagSettingsEditor: clear invalid tags and add root tag");
            m_Target.allTags.Clear();
            m_Target.allTags.Add(GameplayTag.RootTag);
        }
        GameplayTagManager.instance.Clear();
        GameplayTagManager.instance.InsertTagsIntoTree(m_Target.allTags);
    }

    private void CreateRichBoxStyle()
    {
        if (m_RichBoxStyle == null)
        {
            m_RichBoxStyle = new GUIStyle(GUI.skin.box);
            m_RichBoxStyle.richText = true;
        }
    }

    private class TagEditorNode
    {
        public string fullName;
        public string shortName;
        public TagEditorNode parent = null;
        public List<TagEditorNode> children = new List<TagEditorNode>();
        public bool expand = false;

        // if there isn't '.' in text, Split('.') return an array of length 1.
        // root node's depth is always 1.
        public int depth => string.IsNullOrEmpty(fullName) ? 0 : fullName.Split('.').Length;
        public bool isRoot => parent == null;

        public void ApplyShortNameChange()
        {
            if (isRoot)
                return;

            int lastDot = fullName.LastIndexOf('.');
            fullName = new StringBuilder(fullName.Substring(0, lastDot + 1)).Append(shortName).ToString();
        }

        public void MakeShortName(string name)
        {
            if(string.IsNullOrEmpty(name))
                return;

            int lastDot = name.LastIndexOf('.');
            if (lastDot == -1)
                shortName = name;
            else if(lastDot <= name.Length - 2)
                shortName = name.Substring(lastDot + 1);
        }

        public void DeleteChild(TagEditorNode child)
        {
            for (int i = 0; i < children.Count; ++i)
            {
                if (children[i] == child)
                {
                    children.RemoveAt(i);
                    break;
                }
            }

            if(children == null || children.Count == 0)
                expand = false;
        }
    }
}