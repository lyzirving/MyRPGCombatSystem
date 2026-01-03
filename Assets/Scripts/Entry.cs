using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Entry : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("App entry");
        InputManager.instance.Init();
        AnimationEventReceiver.instance.Init();
    }

    private void OnDestroy()
    {
        AnimationEventReceiver.instance.DeInit();
        MonoManager.instance.DeInit();
        InputManager.instance.DeInit();        
    }
}
