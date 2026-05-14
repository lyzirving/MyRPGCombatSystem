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

    private void OnEnable()
    {
        m_Enabled = true;
        GameplayEffect effect = (GameplayEffect)target;
        SyncTagList(effect.grantedTags, m_GrantedTagsView.tags);
        SyncTagList(effect.removedTags, m_RemoveTagsView.tags);
        SyncTagList(effect.requiredTags, m_RequiredTagsView.tags);
        SyncTagList(effect.blockedTags, m_BlockedTagsView.tags);
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

        GameplayEffect effect = (GameplayEffect)target;
        DrawTagListView(effect.grantedTags, m_GrantedTagsView);
        DrawTagListView(effect.removedTags, m_RemoveTagsView);
        DrawTagListView(effect.requiredTags, m_RequiredTagsView);
        DrawTagListView(effect.blockedTags, m_BlockedTagsView);
    }

    private void DrawTagListView(List<GameplayTag> tagList, TagListView view)
    {
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
            DrawTagList(tagList);
        }
    }

    private void DrawTagList(List<GameplayTag> tagList)
    {
        
    }

    private void SyncTagList(List<GameplayTag> src, List<GameplayTag> dst)
    { 
        dst.Clear();

        foreach (GameplayTag tag in src)
        {
            dst.Add(new GameplayTag(tag.name));
        }
    }

    public void OnSelected(GameplayTag tag)
    {
        Debug.Log($"GameplayEffectEditor: OnSelected, tag[{tag.name}], editor enable[{m_Enabled}]");
        m_EditingView = null;
        if (m_Enabled)
        { 
        }
    }

    public void OnCanceled()
    {
        m_EditingView = null;
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