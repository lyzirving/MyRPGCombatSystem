using UnityEditor;
using UnityEngine;

public class GUIDEditorWindow : EditorWindow
{
    private GUIDDataBase database;
    private GUIDEntry entry;

    private string editName;
    private string editCategory;
    private string editDescription;

    public static void ShowWindow(GUIDDataBase db, GUIDEntry guidEntry)
    {
        var window = GetWindow<GUIDEditorWindow>("GUID Editor");
        window.database = db;
        window.entry = guidEntry;
        window.editName = guidEntry.name;
        window.editCategory = guidEntry.category;
        window.editDescription = guidEntry.description;
        window.minSize = new Vector2(400, 200);
    }

    void OnGUI()
    {
        if (database == null || entry == null)
        {
            EditorGUILayout.HelpBox("invalida date", MessageType.Error);
            return;
        }

        EditorGUILayout.LabelField("Edit Guid Information", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("GUID:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(entry.guid.ToString(), EditorStyles.boldLabel, GUILayout.Height(20));

        EditorGUILayout.Space();

        editName = EditorGUILayout.TextField("name", editName);
        editCategory = EditorGUILayout.TextField("category", editCategory);
        editDescription = EditorGUILayout.TextField("description", editDescription);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Save"))
        {
            SaveGuidChanges();
            Close();
        }

        if (GUILayout.Button("Cancel"))
        {
            Close();
        }

        EditorGUILayout.EndHorizontal();
    }

    void SaveGuidChanges()
    {
        database.UpdateEntry(
            entry.guid,
            editName,
            editCategory,
            editDescription
        );

        Debug.Log($"GUID is updated: {editName}");
    }
}
