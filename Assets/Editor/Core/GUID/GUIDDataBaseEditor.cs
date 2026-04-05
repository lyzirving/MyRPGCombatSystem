using UnityEditor;
using UnityEngine;

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
            EditorGUILayout.TextField("Guid", $"{entry.guid}", GUILayout.ExpandWidth(true));
            EditorGUILayout.TextField("Name", entry.name, GUILayout.ExpandWidth(true));                      
            EditorGUILayout.TextField("Last Update Time", $"{entry.createdTime}", GUILayout.ExpandWidth(true));

            if (i < entries.Count - 1)
                GUILayout.Space(10);
        }
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Edit Guid Database"))
            GUIDGeneratorWindow.ShowWindow();
    }
}
