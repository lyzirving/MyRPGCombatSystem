using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class Entry : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("App entry");
        InputManager.Init();
        AnimationEventReceiver.Init();
    }

    public void ReloadScene()
    {
        string active = SceneManager.GetActiveScene().name;
        Debug.Log($"ReloadScene[{active}]");
        SceneManager.LoadScene(active);
    }
}
