using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameSettings : SingletonMono<GameSettings>
{
    public static CharacterConfig characterConfig;
    private static AsyncOperationHandle<CharacterConfig> k_ConfigHandle;

    public override void OnInit()
    {
        if (characterConfig == null)
        {
            k_ConfigHandle = Addressables.LoadAssetAsync<CharacterConfig>("Game/CharacterConfig");
            characterConfig = k_ConfigHandle.WaitForCompletion();
        }
    }

    public override void OnDeInit()
    {
        characterConfig = null;
        Addressables.Release(k_ConfigHandle);
    }
}
