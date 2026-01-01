using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIInputDetector : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CursorManager.instance.IsCursorHide())
        {
            CursorManager.instance.HideCursor();
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && CursorManager.instance.IsCursorHide())
        {
            CursorManager.instance.ShowCursor();
        }
    }
}
