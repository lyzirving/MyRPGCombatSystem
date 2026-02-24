using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface GUIDSelectionChange
{
    public void OnGUIDSelectionChange(GUIDEntry entry);
}

public class GUIDSelectionWindow : EditorWindow
{
    private GUIDDataBase m_Database;

    private List<string> m_NameList = new List<string>();
    private int m_Selection = 0;    
    private bool m_FirstUpdate = true;

    private int m_InputGuid = -1;
    private GUIDSelectionChange m_Callback = null;

    public static void ShowWindow(GUIDSelectionChange callback = null, int currentGUID = -1)
    {
        var window = GetWindow<GUIDSelectionWindow>("GUID Selector");
        //Note: Init() is called after OnEnable() every time when Window is shown.
        window.Init(callback, currentGUID);
        window.minSize = new Vector2(400, 200);
    }

    private void OnEnable()
    {
        //Debug.Log("GUIDSelectionWindow OnEnable");
        m_Database = GUIDGeneratorWindow.database;
        m_FirstUpdate = true;        
        m_Selection = 0;        
    }

    private void OnDisable()
    {
        m_Database = null;
        m_Callback = null;
        m_NameList.Clear();
    }

    private void OnGUI()
    {
        if (m_Database == null)
        {
            EditorGUILayout.HelpBox("invalida date", MessageType.Error);
            return;
        }
        EditorGUILayout.LabelField("Entry count", $"{m_Database.count}", EditorStyles.textField);
        int newSelection = EditorGUILayout.Popup("Guid Selection", m_Selection, m_NameList.ToArray());
        var currentEntry = m_Database.allEntries[newSelection];

        if (newSelection != m_Selection || m_FirstUpdate)
        {
            m_Selection = newSelection;
            m_Callback?.OnGUIDSelectionChange(currentEntry);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Information", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.TextField("GUID", $"{currentEntry.guid}", EditorStyles.textField);
        EditorGUILayout.TextField("Name", $"{currentEntry.name}", EditorStyles.textField);
        EditorGUILayout.TextField("Created Time", $"{currentEntry.createdTime}", EditorStyles.textField);
        EditorGUILayout.EndVertical();

        if (m_FirstUpdate)
            m_FirstUpdate = false;
    }

    private void Init(GUIDSelectionChange callback, int currentGUID)
    {
        //Debug.Log($"GUIDSelectionWindow Init, guid[{currentGUID}], callback[{callback}]");
        m_Callback = callback;
        m_InputGuid = currentGUID;
        if (m_Database != null)
        {
            if (m_InputGuid >= 0)
                m_Selection = m_Database.FindByGuid(m_InputGuid).guid;

            var entries = m_Database.allEntries;
            for (int i = 0; i < entries.Count; ++i)
                m_NameList.Add(entries[i].name);
        }
    }
}
