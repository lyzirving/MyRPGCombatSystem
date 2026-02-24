using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GUIDGeneratorWindow : EditorWindow
{
    private static readonly string m_CodeGenerateDirectory = Application.dataPath + "/Scripts/Core/GUID/";
    private static readonly string m_CodeFileName = "GUIDConsts.cs";

    private static GUIDDataBase m_CurrentDataBase;
    private Vector2 m_ScrollPosition;

    // fileds for newly-added guid
    private string m_NewGUIDName = "";
    private string m_NewGUIDCategory = "default";
    private string m_NewGUIDDescription = "";

    // search and filter
    private string m_SearchFilter = "";
    private string m_SelectedCategory = "All";

    public static GUIDDataBase database 
    {
        get
        {
            if (m_CurrentDataBase == null)
            { 
                LoadUniqueDatabase();
            }
            return m_CurrentDataBase;
        }
    }

    [MenuItem("Tools/Guid Generator", false, 100)]
    public static void ShowWindow()
    {
        var window = GetWindow<GUIDGeneratorWindow>("Guid Generator");
        window.minSize = new Vector2(600, 400);        
    }

    private void OnEnable()
    {
        LoadOrCreateDatabase();
    }

    void OnGUI()
    {
        if (m_CurrentDataBase == null)
        {
            EditorGUILayout.HelpBox("Fail to find Guid database", MessageType.Error);
            if (GUILayout.Button("Create new Guid database"))
                CreateNewDatabase();
            return;
        }

        DrawHeader();
        DrawAddNewSection();
        DrawFilterSection();
        DrawGUIDList();
        DrawBottomButtons();
    }

    static void LoadUniqueDatabase()
    {
        if (m_CurrentDataBase != null)
            return;

        // search for existing database
        string[] database = AssetDatabase.FindAssets("t:GUIDDataBase");
        if (database.Length > 0)
        {
            // Only one database is allowed in the project
            string path = AssetDatabase.GUIDToAssetPath(database[0]);
            m_CurrentDataBase = AssetDatabase.LoadAssetAtPath<GUIDDataBase>(path);
        }
    }

    static void LoadOrCreateDatabase()
    {
        LoadUniqueDatabase();

        if (m_CurrentDataBase == null)
            CreateNewDatabase();
    }

    static void CreateNewDatabase()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create GUID database",
            "GUIDDatabase",
            "asset",
            "Select save path"
        );

        if (!string.IsNullOrEmpty(path))
        {
            m_CurrentDataBase = CreateInstance<GUIDDataBase>();
            AssetDatabase.CreateAsset(m_CurrentDataBase, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"succeed to create new Guid database: {path}");
        }
    }

    #region Draw Function
    void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUILayout.Label("Database: ", GUILayout.Width(60));
        m_CurrentDataBase = (GUIDDataBase)EditorGUILayout.ObjectField(
            m_CurrentDataBase, typeof(GUIDDataBase), false, GUILayout.Width(400));
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Entry count: {m_CurrentDataBase.count}", GUILayout.Width(100));

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

    void DrawAddNewSection()
    {
        EditorGUILayout.LabelField("Add new Guid", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("Box");

        m_NewGUIDName = EditorGUILayout.TextField("Name", m_NewGUIDName);
        m_NewGUIDCategory = EditorGUILayout.TextField("Category", m_NewGUIDCategory);
        m_NewGUIDDescription = EditorGUILayout.TextField("Description", m_NewGUIDDescription);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate GUID", GUILayout.Height(30)))
        {
            AddNewGUID();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    void DrawFilterSection()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Search: ", GUILayout.Width(45));
        m_SearchFilter = EditorGUILayout.TextField(m_SearchFilter);

        GUILayout.FlexibleSpace();

        EditorGUILayout.LabelField("Category: ", GUILayout.Width(60));
        List<string> categories = GetCategories();
        int selectedIndex = categories.IndexOf(m_SelectedCategory);
        int newIndex = EditorGUILayout.Popup(selectedIndex, categories.ToArray(), GUILayout.Width(200));

        if (newIndex >= 0 && newIndex < categories.Count)
        {
            m_SelectedCategory = categories[newIndex];
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

    void DrawGUIDList()
    {
        EditorGUILayout.LabelField("Guid List", EditorStyles.boldLabel);

        List<GUIDEntry> filteredEntries = GetFilteredEntries();

        m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, GUILayout.ExpandHeight(true));

        for (int i = 0; i < filteredEntries.Count; i++)
        {
            DrawGUIDEntry(i, filteredEntries[i]);
        }

        if (filteredEntries.Count == 0)
        {
            EditorGUILayout.HelpBox("Do not find any matched GUID", MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
    }

    void DrawGUIDEntry(int index, GUIDEntry entry)
    {
        EditorGUILayout.BeginVertical("Box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Name:", GUILayout.Width(40));
        EditorGUILayout.LabelField(entry.name, EditorStyles.boldLabel, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();             

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("GUID:", GUILayout.Width(40));
        EditorGUILayout.SelectableLabel(entry.guid.ToString(), EditorStyles.boldLabel, GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField($"Category: {entry.category}", GUILayout.Width(150));
        EditorGUILayout.LabelField("Description: " + entry.description, EditorStyles.wordWrappedLabel);     
        EditorGUILayout.LabelField($"Created Time: {entry.createdTime}", EditorStyles.miniLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(60)))
        {
            ShowEditWindow(entry);
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Delete", GUILayout.Width(60)))
        {
            if (EditorUtility.DisplayDialog("Confirm Delete", $"Delete GUID '{entry.name}'?", "Confirm", "Cancel"))
            {
                m_CurrentDataBase.RemoveByGuid(entry.guid);
                Repaint();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        if (index < m_CurrentDataBase.count - 1)
        {
            EditorGUILayout.Separator();
        }
    }

    void DrawBottomButtons()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate Code", GUILayout.Height(30)))
        {
            GenerateCode();
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Clear Database", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirm Clear", "Clear all the Guid?", "Clear", "Cancel"))
            {
                m_CurrentDataBase.ClearAll();
                Repaint();
            }
        }

        if (GUILayout.Button("Refresh", GUILayout.Height(30)))
        {
            LoadOrCreateDatabase();
            Repaint();
        }

        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region Main Function

    GUIDEntry AddNewGUID()
    {
        if (string.IsNullOrEmpty(m_NewGUIDName))
        {
            EditorUtility.DisplayDialog("Erro", "Guid field name should be null!", "Confirm");
            return null;
        }

        var existing = m_CurrentDataBase.FindByName(m_NewGUIDName);
        if (existing != null)
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "Guid already exists",
                $"Name '{m_NewGUIDName}' already exist, should we override it?",
                "Override",
                "Cancel"
            );

            if (!overwrite) return null;

            m_CurrentDataBase.RemoveByGuid(existing.guid);
        }

        var entry = m_CurrentDataBase.AddGUID(
            m_NewGUIDName,
            m_NewGUIDCategory,
            m_NewGUIDDescription
        );

        // Reset input filed
        m_NewGUIDName = "";
        m_NewGUIDDescription = "";

        Repaint();
        return entry;
    }    

    List<string> GetCategories()
    {
        HashSet<string> categories = new HashSet<string> { "All" };
        foreach (var entry in m_CurrentDataBase.allEntries)
        {
            if (!string.IsNullOrEmpty(entry.category))
                categories.Add(entry.category);
        }
        return categories.OrderBy(c => c).ToList();
    }

    List<GUIDEntry> GetFilteredEntries()
    {
        var entries = m_CurrentDataBase.allEntries;

        // filter by categoty
        if (m_SelectedCategory != "All")
        {
            entries = entries.Where(e => e.category == m_SelectedCategory).ToList();
        }

        // filter by input text
        if (!string.IsNullOrEmpty(m_SearchFilter))
        {
            string filter = m_SearchFilter.ToLower();
            entries = entries.Where(e =>
                e.name.ToLower().Contains(filter) ||
                e.description.ToLower().Contains(filter) ||
                e.guid.ToString().ToLower().Contains(filter)
            ).ToList();
        }

        return entries;
    }

    void ShowEditWindow(GUIDEntry entry)
    {
        GUIDEditorWindow.ShowWindow(m_CurrentDataBase, entry);
    }    

    void GenerateCode()
    {
        string rootDir = Application.dataPath + "/Scripts/Core/GUID/";
        string path = EditorUtility.SaveFilePanel(
            "Export As Code",
            m_CodeGenerateDirectory,
            m_CodeFileName,
            "cs"
        );

        if (!string.IsNullOrEmpty(path))
        {
            m_CurrentDataBase.GenerateCodeFile(path);
        }
    }
    #endregion
}
