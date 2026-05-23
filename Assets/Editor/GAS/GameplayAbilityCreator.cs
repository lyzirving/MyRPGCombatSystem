#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.IO;

public class GameplayAbilityCreator
{
    [MenuItem("Assets/Create/GAS/GameplayAbility", priority = 10)]
    private static void CreateGameplayAbility()
    {
        // 1. create instance
        var data = ScriptableObject.CreateInstance<GameplayAbility>();
        // 2. generate guid
        data.SetUniqueID(System.Guid.NewGuid().ToString());

        // 3. confirm the save path
        string path = GetSelectedPathOrFallback();
        string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, "NewGameplayAbility.asset"));

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
