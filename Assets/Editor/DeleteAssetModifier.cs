using UnityEditor;
using UnityEngine;

public class DeleteAssetModifier : AssetModificationProcessor
{
    static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
    {
        if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(GameplayTagDatabase))
        {
            Debug.Log($"Deleting GameplayTagDatabase[{assetPath}] is detected");
            GameplayTagManager.instance.Clear();
        }
        // Return DidNotDelete and let the Engine do its work.
        // User code should do the delete operation here.
        return AssetDeleteResult.DidNotDelete;
    }
}
