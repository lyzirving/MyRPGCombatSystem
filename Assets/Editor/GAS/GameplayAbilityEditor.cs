using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameplayAbility), true)]
public class GameplayAbilityEditor : Editor, IGameplayTagSelection
{
    private enum EEditingSection
    {
        None = 0,
        CoolDown,
        RequeiredTagList,
        BlockedTagList
    }

    private bool m_Enabled;
    private EEditingSection m_Editing = EEditingSection.None;
    private GameplayAbility m_Ability = null;
    private bool m_RequeiredTagExpand = false;
    private bool m_BlockedTagExpand = false;
    private List<GameplayTag> m_RemoveList = new List<GameplayTag>();

    private void OnEnable()
    {
        m_Enabled = true;
        m_Ability = target as GameplayAbility;
        m_RequeiredTagExpand = false;
        m_BlockedTagExpand = false;
    }

    private void OnDisable()
    {
        m_Enabled = false;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();        

        DrawCooldownSection(m_Ability);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gameplay Tag Settings", EditorStyles.boldLabel);
        DrawRequiredTagsSection(m_Ability);
        DrawBlockedTagsSection(m_Ability);

        DrawSaveButton();
    }    

    private void DrawCooldownSection(GameplayAbility ability)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Cooldown Settings", EditorStyles.boldLabel);

        // Cool down tag
        EditorGUILayout.BeginHorizontal();        
        GUILayout.Label("Cooldown Tag", GUILayout.ExpandWidth(false));
        GUILayout.Label(m_Ability.cooldownTag.isValid ? m_Ability.cooldownTag.simpleName: "", 
            "TextField", GUILayout.ExpandWidth(true));
        if (GUILayout.Button("●", GUILayout.Width(22)))
        {
            m_Editing = EEditingSection.CoolDown;
            GameplayTagSelectorWindow.ShowWindow(this);
        }
        if (GUILayout.Button("×", GUILayout.Width(22)))
        {
            m_Ability.cooldownTag = new GameplayTag();
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.EndHorizontal();

        float duration = EditorGUILayout.Slider("Cooldown Duration", ability.cooldownDuration, 0f, 1000f);
        if(duration != ability.cooldownDuration)
        {
            ability.cooldownDuration = duration;
            EditorUtility.SetDirty(target);
        }

        var obj = EditorGUILayout.ObjectField("CooldownEffect", ability.cooldownEffect, typeof(GameplayEffect), false);
        if (obj != ability.cooldownEffect)
        {
            ability.cooldownEffect = (obj as GameplayEffect);
            EditorUtility.SetDirty(target);
        }
    }

    private void DrawRequiredTagsSection(GameplayAbility ability)
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("RequiredTags", GUILayout.ExpandWidth(true));

        GUILayout.Label($"{ability.requiredTags.Count}", "TextField", GUILayout.ExpandWidth(false));

        if (GUILayout.Button(m_RequeiredTagExpand ? "▼" : "▶", GUILayout.Width(22)))
        {
            m_RequeiredTagExpand = !m_RequeiredTagExpand;
        }

        if (GUILayout.Button("+", GUILayout.Width(22)))
        {
            m_Editing = EEditingSection.RequeiredTagList;
            GameplayTagSelectorWindow.ShowWindow(this);
        }

        EditorGUILayout.EndHorizontal();

        if (m_RequeiredTagExpand)
        {
            DrawTagList(ability.requiredTags);
        }
    }    

    private void DrawBlockedTagsSection(GameplayAbility ability)
    {        
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("BlockedTags", GUILayout.ExpandWidth(true));

        GUILayout.Label($"{ability.blockedTags.Count}", "TextField", GUILayout.ExpandWidth(false));

        if (GUILayout.Button(m_BlockedTagExpand ? "▼" : "▶", GUILayout.Width(22)))
        {
            m_BlockedTagExpand = !m_BlockedTagExpand;
        }

        if (GUILayout.Button("+", GUILayout.Width(22)))
        {
            m_Editing = EEditingSection.BlockedTagList;
            GameplayTagSelectorWindow.ShowWindow(this);
        }

        EditorGUILayout.EndHorizontal();

        if (m_BlockedTagExpand)
        {
            DrawTagList(ability.blockedTags);
        }
    }

    private void DrawTagList(List<GameplayTag> tags)
    {
        m_RemoveList.Clear();
        for (int i = 0; i < tags.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(tags[i].simpleName, "TextField", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("×", GUILayout.Width(22)))
            {
                m_RemoveList.Add(tags[i]);
            }
            EditorGUILayout.EndHorizontal();
        }

        if (m_RemoveList.Count > 0)
        {
            EditorUtility.SetDirty(target);
            foreach (var toBeRemoved in m_RemoveList)
            {
                for (int i = tags.Count - 1; i >= 0; --i)
                {
                    if (tags[i].Equals(toBeRemoved))
                    {
                        tags.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }

    private void DrawSaveButton()
    {
        if (!EditorUtility.IsDirty(target)) EditorGUI.BeginDisabledGroup(true);
        if (GUILayout.Button("Apply", GUILayout.Width(60)))
            AssetDatabase.SaveAssetIfDirty(target);
        if (!EditorUtility.IsDirty(target)) EditorGUI.EndDisabledGroup();
    }

    public void OnTagSelected(GameplayTag tag)
    {
        if(!m_Enabled)
            return;

        switch (m_Editing)
        {
            case EEditingSection.CoolDown:
                if (m_Ability.cooldownTag != tag)
                {                    
                    m_Ability.cooldownTag = tag;
                    EditorUtility.SetDirty(target);
                }  
                break;
            case EEditingSection.RequeiredTagList:
                {
                    var found = m_Ability.requiredTags.Find(item => item.Equals(tag));
                    if (!found.isValid)
                    {
                        m_Ability.requiredTags.Add(tag);
                        m_RequeiredTagExpand = true;
                        EditorUtility.SetDirty(target);
                    }
                    break;
                }
            case EEditingSection.BlockedTagList:
                {
                    var found = m_Ability.blockedTags.Find(item => item.Equals(tag));
                    if (!found.isValid)
                    {
                        m_Ability.blockedTags.Add(tag);
                        m_BlockedTagExpand = true;
                        EditorUtility.SetDirty(target);
                    }
                    break;
                }
            default:
                break;
        }
    }

    public void OnTagCanceled()
    {
    }
}
