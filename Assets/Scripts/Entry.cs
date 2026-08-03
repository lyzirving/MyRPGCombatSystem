using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Entry : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("App entry");
        GameSettings.Init();
        InputManager.Init();
        AnimationConsts.Init();
        AnimationEventReceiver.Init();
        GameplayTagManager.Init();
        VFXManager.Init();
        AIManager.Init();
        GhostPool.Init();
    }
}
