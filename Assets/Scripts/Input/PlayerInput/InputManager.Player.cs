using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class InputManager
{
    public delegate void RunToggleChange(bool shouldRun);

    private bool m_ShouldPlayerRun = true;
    private bool m_IsJumpPerformed = false;
    private WaitForEndOfFrame m_WaitForEndOfFrame = new WaitForEndOfFrame();

    public bool shouldPlayerRun { get => m_ShouldPlayerRun; }
    public event RunToggleChange runToggleChange;

    public Vector2 playerMovement
    {
        get => m_PlayerActionMap.PlayerAction.Move.ReadValue<Vector2>();
    }

    public bool isPlayerMoving { get => playerMovement != Vector2.zero; }
    public bool isPlayerJumpPerformed { get => m_IsJumpPerformed; }

    private void OnSwitchRunToggle(InputAction.CallbackContext context)
    {
        m_ShouldPlayerRun = !m_ShouldPlayerRun;
        Debug.Log($"OnSwitchRunToggle[{m_ShouldPlayerRun}]");
        runToggleChange?.Invoke(m_ShouldPlayerRun);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        m_IsJumpPerformed = true;
        MonoManager.Run(OnJumpCancel());
    }

    private IEnumerator OnJumpCancel()
    {
        yield return m_WaitForEndOfFrame;
        m_IsJumpPerformed = false;
    }
}
