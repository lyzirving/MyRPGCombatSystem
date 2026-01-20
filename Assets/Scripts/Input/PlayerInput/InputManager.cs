using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class InputManager : Singleton<InputManager>
{
    private bool m_Enabled = false;
    private IA_Player m_PlayerActionMap;

    public bool isEnabled { get => m_Enabled; }

    public override void OnInit()
    {
        m_PlayerActionMap = new IA_Player();
        m_PlayerActionMap.PlayerAction.RunToggle.performed += OnSwitchRunToggle;
        m_PlayerActionMap.PlayerAction.Jump.performed += OnJumpPerformed;
        m_PlayerActionMap.PlayerAction.Roll.performed += OnRollPerformed;
    }

    public override void OnDeInit()
    {
        m_PlayerActionMap.PlayerAction.RunToggle.performed -= OnSwitchRunToggle;
        m_PlayerActionMap.PlayerAction.Jump.performed -= OnJumpPerformed;
        m_PlayerActionMap.PlayerAction.Roll.performed -= OnRollPerformed;
        m_PlayerActionMap = null;
    }

    public void Enable()
    {
        m_PlayerActionMap.Enable();        
        m_Enabled = true;
    }

    public void Disable()
    {        
        m_PlayerActionMap.Disable();
        m_Enabled = false;
    }

    public void DisableActionForTime(InputAction action, float time)
    {
        MonoManager.Run(DisableActionForTimeCoroutine(action, time));
    }

    private IEnumerator DisableActionForTimeCoroutine(InputAction action, float time)
    {
        action?.Disable();
        yield return new WaitForSeconds(time);
        action?.Enable();
    }    
}
