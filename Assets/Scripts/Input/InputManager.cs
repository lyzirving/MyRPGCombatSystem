using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    private bool m_Enabled = false;
    private IA_Player m_PlayerActionMap;

    public bool isEnabled { get => m_Enabled; }
    public IA_Player.PlayerActions playerActions { get => m_PlayerActionMap.Player; }

    public override void OnInit()
    {
        Debug.Log("InputManager OnInit");
        m_PlayerActionMap = new IA_Player();
    }

    public override void OnDeInit()
    {
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
}
