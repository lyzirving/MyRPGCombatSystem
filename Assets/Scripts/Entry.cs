using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Entry : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("App entry");
        InputManager.Init();        
        AnimationEventReceiver.Init();
    }

    private void OnDestroy()
    {
        Debug.Log("App destroy");
        AnimationEventReceiver.DeInit();
        MonoManager.DeInit();
        InputManager.DeInit();        
    }
}
