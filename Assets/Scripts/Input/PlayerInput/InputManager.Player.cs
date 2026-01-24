using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class InputManager
{
    private bool m_ShouldPlayerRun = true;
    private bool m_IsJumpPerformed = false;
    private bool m_IsRollPerformed = false;
    private bool m_IsAttackPerformed = false;
    private WaitForEndOfFrame m_WaitForEndOfFrame = new WaitForEndOfFrame();

    public bool shouldPlayerRun { get => m_ShouldPlayerRun; }

    public Vector2 playerMovement
    {
        get => m_PlayerActionMap.PlayerAction.Move.ReadValue<Vector2>();
    }

    public bool isPlayerMoving { get => playerMovement != Vector2.zero; }
    public bool isPlayerJumpPerformed { get => m_IsJumpPerformed; }
    public bool isPlayerRollPerformed { get => m_IsRollPerformed; }
    public bool isPlayerAttackPerformed { get => m_IsAttackPerformed; }

    private void OnSwitchRunToggle(InputAction.CallbackContext context)
    {
        m_ShouldPlayerRun = !m_ShouldPlayerRun;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        m_IsJumpPerformed = true;
        MonoManager.Run(OnJumpCancel());
    }

    private void OnRollPerformed(InputAction.CallbackContext context)
    {
        m_IsRollPerformed = true;
        MonoManager.Run(OnRollCancel());
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        m_IsAttackPerformed = true;
        MonoManager.Run(OnAttackCancel());
    }

    private IEnumerator OnJumpCancel()
    {
        yield return m_WaitForEndOfFrame;
        m_IsJumpPerformed = false;
    }

    private IEnumerator OnRollCancel()
    {
        yield return m_WaitForEndOfFrame;
        m_IsRollPerformed = false;
    }

    private IEnumerator OnAttackCancel()
    {
        yield return m_WaitForEndOfFrame;
        m_IsAttackPerformed = false;
    }
}
