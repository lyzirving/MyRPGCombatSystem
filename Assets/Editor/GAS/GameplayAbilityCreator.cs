#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.IO;

internal static class GameplayAbilityCreator
{
    public static void CreateGameplayAbility<T>(string defaultName = "") where T : GameplayAbility
    {
        // 1. create instance
        var data = ScriptableObject.CreateInstance<T>();
        // 2. generate guid
        data.SetUniqueID(System.Guid.NewGuid().ToString());

        // 3. confirm the save path
        string path = GetSelectedPathOrFallback();
        if (string.IsNullOrEmpty(defaultName))
            defaultName = $"New{typeof(T).Name}";
        string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, $"{defaultName}.asset"));

        // 4. create asset
        AssetDatabase.CreateAsset(data, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 5. focus the created item
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = data;
    }

    private static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            // Find the first selection asset's path
            path = AssetDatabase.GetAssetPath(obj);
            if (File.Exists(path))
                path = Path.GetDirectoryName(path);
            break;
        }
        return path;
    }
}

#endif
