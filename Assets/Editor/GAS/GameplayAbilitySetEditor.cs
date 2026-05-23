using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(GameplayAbilitySet))]
public class GameplayAbilitySetEditor : Editor
{
    private List<GameplayAbility> m_Abilities = new List<GameplayAbility>();
    private ReorderableList m_ReorderableList = null;
    private GameplayAbilitySet m_Target;

    private void OnEnable()
    {
        m_Target = (GameplayAbilitySet)target;
        m_Abilities.Clear();
        for (int i = 0; i < m_Target.abilities.Count; i++)
            m_Abilities.Add(m_Target.abilities[i]);

        m_ReorderableList = new ReorderableList(m_Abilities, typeof(GameplayAbility), true, true, true, true);
        m_ReorderableList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Ability List");
        m_ReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            rect.y += 2;
            var element = m_Abilities[index];            
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element, typeof(GameplayAbility), false);
            if (EditorGUI.EndChangeCheck())
            {
                var ability = newValue as GameplayAbility;
                m_Abilities[index] = ability;
                m_Target.Add(ability);
                EditorUtility.SetDirty(target);
            }
        };

        m_ReorderableList.onAddCallback = (list) =>
        {
            m_Abilities.Add(null);
        };

        m_ReorderableList.onRemoveCallback = (list) =>
        {
            if (EditorUtility.DisplayDialog("Confirm to delete", "Are you sure to remove this ability", "Yes", "No"))
            {
                var ability = m_Abilities[list.index];
                m_Target.Remove(ability);
                m_Abilities.RemoveAt(list.index);
                EditorUtility.SetDirty(target);
            }
        };
    }

    public override void OnInspectorGUI()
    {
        m_ReorderableList.DoLayoutList();
        DrawSaveButton();
    }

    private void DrawSaveButton()
    {
        if (!EditorUtility.IsDirty(target)) EditorGUI.BeginDisabledGroup(true);
        if (GUILayout.Button("Apply", GUILayout.Width(60)))
            AssetDatabase.SaveAssetIfDirty(target);
        if (!EditorUtility.IsDirty(target)) EditorGUI.EndDisabledGroup();
    }
}
