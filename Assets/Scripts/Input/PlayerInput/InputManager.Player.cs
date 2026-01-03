using System;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class InputManager
{
    public delegate void RunToggleChange(bool shouldRun);

    private bool m_ShouldPlayerRun = true;

    public bool shouldPlayerRun { get => m_ShouldPlayerRun; }
    public event RunToggleChange runToggleChange;

    public Vector2 playerMovement
    {
        get => m_PlayerActionMap.PlayerAction.Move.ReadValue<Vector2>();
    }

    public bool isPlayerMoving
    {
        get => playerMovement != Vector2.zero;
    }

    private void OnSwitchRunToggle(InputAction.CallbackContext context)
    {
        m_ShouldPlayerRun = !m_ShouldPlayerRun;
        runToggleChange?.Invoke(m_ShouldPlayerRun);
    }
}
