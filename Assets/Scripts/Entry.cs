using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Entry : MonoBehaviour
{
    void Start()
    {
        Debug.Log("App entry");
        InputManager.Init();        
        AnimationEventReceiver.Init();
    }
}
