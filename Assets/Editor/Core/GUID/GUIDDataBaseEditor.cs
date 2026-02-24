using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CustomEditor(typeof(GUIDDataBase))]
public class GUIDDataBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUIDDataBase database = (GUIDDataBase)target;

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", database, database.GetType(), false);
        EditorGUI.EndDisabledGroup();

        var entries = database.allEntries;       
        GUILayout.Label($"Modify Time: {database.modifyTime:yyyy-MM-dd HH:mm}", EditorStyles.boldLabel);
        GUILayout.Label($"Entry Num: {entries.Count}", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("Box");
        for (int i = 0; i < entries.Count; ++i)
        { 
            var entry = entries[i];
            EditorGUILayout.LabelField($"Entry{i}", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Guid", EditorStyles.boldLabel);
            EditorGUILayout.TextField($"{entry.guid}", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Name", EditorStyles.boldLabel);
            EditorGUILayout.TextField(entry.name, GUILayout.ExpandWidth(true));            
            EditorGUILayout.EndHorizontal();            

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Last Update Time", EditorStyles.boldLabel);
            EditorGUILayout.TextField($"{entry.createdTime}", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            if (i < entries.Count - 1)
                GUILayout.Space(10);
        }
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Edit Guid Database"))
            GUIDGeneratorWindow.ShowWindow();
    }
}
