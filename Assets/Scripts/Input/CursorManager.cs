using UnityEngine;

public class CursorManager : Singleton<CursorManager>
{
    public void HideCursor()
    {
        Debug.Log("HideCursor");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        InputManager.instance.Enable();
    }

    public void ShowCursor()
    {
        Debug.Log("ShowCursor");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        InputManager.instance.Disable();
    }

    public bool IsCursorHide()
    {
        return Cursor.visible == false;
    }
}
