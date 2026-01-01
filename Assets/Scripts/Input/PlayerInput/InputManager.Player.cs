using UnityEngine;

public partial class InputManager
{
    public Vector2 playerMovement
    {
        get => m_PlayerActionMap.PlayerAction.Move.ReadValue<Vector2>();
    }

    public bool isPlayerMoving
    {
        get => playerMovement != Vector2.zero;
    }
}
