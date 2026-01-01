using UnityEngine;

public partial class InputManager
{
    public Vector2 cameraMovement
    {
        get => m_PlayerActionMap.PlayerAction.CameraMove.ReadValue<Vector2>();
    }

    public bool isCameraMoving
    {
        get => cameraMovement != Vector2.zero;
    }
}
