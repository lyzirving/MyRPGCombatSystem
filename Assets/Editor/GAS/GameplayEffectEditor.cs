using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameplayEffect))]
public class GameplayEffectEditor : Editor, IGameplayTagSelection
{
    private bool m_Enabled;
    private TagListView m_GrantedTagsView = new TagListView("GrantedTags");
    private TagListView m_RemoveTagsView = new TagListView("RemoveTags");
    private TagListView m_RequiredTagsView = new TagListView("RequiredTags");
    private TagListView m_BlockedTagsView = new TagListView("BlockedTags");

    private TagListView m_EditingView = null;    
    private List<GameplayTag> m_EditingRemoveList = new List<GameplayTag>();

    private void OnEnable()
    {
        m_Enabled = true;
        GameplayEffect effect = (GameplayEffect)target;
        SyncTagList(effect.grantedTags, m_GrantedTagsView.tags);
        SyncTagList(effect.removedTags, m_RemoveTagsView.tags);
        SyncTagList(effect.requiredTags, m_RequiredTagsView.tags);
        SyncTagList(effect.blockedTags, m_BlockedTagsView.tags);

        m_GrantedTagsView.expand = m_GrantedTagsView.tags.Count > 0;
        m_RemoveTagsView.expand = m_RemoveTagsView.tags.Count > 0;
        m_RequiredTagsView.expand = m_RequiredTagsView.tags.Count > 0;
        m_BlockedTagsView.expand = m_BlockedTagsView.tags.Count > 0;
    }

    private void OnDisable()
    {
        m_Enabled = false;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);

        bool hierarchyChange = false;
        GameplayEffect effect = (GameplayEffect)target;
        DrawTagListView(m_GrantedTagsView, ref hierarchyChange);
        DrawTagListView(m_RemoveTagsView, ref hierarchyChange);
        DrawTagListView(m_RequiredTagsView, ref hierarchyChange);
        DrawTagListView(m_BlockedTagsView, ref hierarchyChange);

        if (hierarchyChange)
            ApplyChangeToTarget();

        DrawSaveButton();
    }

    private void DrawSaveButton()
    {
        if (!EditorUtility.IsDirty(target)) EditorGUI.BeginDisabledGroup(true);
        if (GUILayout.Button("Apply", GUILayout.Width(60)))
            AssetDatabase.SaveAssetIfDirty(target);
        if (!EditorUtility.IsDirty(target)) EditorGUI.EndDisabledGroup();
    }

    private void DrawTagListView(TagListView view, ref bool hierarchyChange)
    {
        var scrollPos = Vector2.zero;
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(view.expand ? "▼" : "▶", GUILayout.Width(22)))
        {
            view.expand = !view.expand;
        }

        GUILayout.Label(view.label);

        // add button
        if (GUILayout.Button("+", GUILayout.Width(22)))
        {
            m_EditingView = view;
            GameplayTagSelectorWindow.ShowWindow(this);
        }

        EditorGUILayout.EndHorizontal();

        if (view.expand)
        {
            DrawTagList(view.tags, ref hierarchyChange);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawTagList(List<GameplayTag> tagList, ref bool hierarchyChange)
    {
        if (tagList == null || tagList.Count == 0)
            return;

        m_EditingRemoveList.Clear();
        for (int i = 0; i < tagList.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(tagList[i].simpleName, "TextField", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("×", GUILayout.Width(22)))
            {
                m_EditingRemoveList.Add(tagList[i]);
                Debug.Log($"add to remove list[{tagList[i]}]");
                hierarchyChange = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        foreach (var remove in m_EditingRemoveList)
        {
            foreach (var tag in tagList)
            {
                if (remove.Equals(tag))
                {
                    tagList.Remove(tag);
                    break;
                }
            }
        }
    }

    private void SyncTagList(List<GameplayTag> src, List<GameplayTag> dst)
    { 
        dst.Clear();

        foreach (GameplayTag tag in src)
        {
            dst.Add(new GameplayTag(tag.name));
        }
    }

    public void OnTagSelected(GameplayTag tag)
    {
        Debug.Log($"GameplayEffectEditor: OnTagSelected, tag[{tag.name}], editor enable[{m_Enabled}]");        
        if (m_Enabled && m_EditingView != null)
        {
            if (!m_EditingView.tags.Contains(tag))
            {
                m_EditingView.expand = true;
                m_EditingView.tags.Add(tag);
                ApplyChangeToTarget();
            }
        }
        m_EditingView = null;
    }

    public void OnTagCanceled()
    {
        m_EditingView = null;
    }

    private void ApplyChangeToTarget()
    {
        GameplayEffect effect = (GameplayEffect)target;
        effect.grantedTags.Clear();
        effect.grantedTags.AddRange(m_GrantedTagsView.tags);

        effect.removedTags.Clear();
        effect.removedTags.AddRange(m_RemoveTagsView.tags);

        effect.requiredTags.Clear();
        effect.requiredTags.AddRange(m_RequiredTagsView.tags);

        effect.blockedTags.Clear();
        effect.blockedTags.AddRange(m_BlockedTagsView.tags);

        EditorUtility.SetDirty(target);        
    }

    private class TagListView
    {
        public string label;
        public bool expand = false;        
        public List<GameplayTag> tags = new List<GameplayTag>();

        public TagListView(string label)
        { 
            this.label = label;
        }
    }
}