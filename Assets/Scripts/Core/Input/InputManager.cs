using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    private bool m_Enabled = false;
    private IA_Player m_PlayerAction;

    public IA_Player playerAction { get => m_PlayerAction; }
    public bool isEnabled { get => m_Enabled; }

    public override void Init()
    {
        m_PlayerAction = new IA_Player();
    }

    public void Enable()
    {
        m_PlayerAction?.Enable();        
        m_Enabled = (m_PlayerAction != null);
    }

    public void Disable()
    {
        m_PlayerAction?.Disable();
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
