using System.Text;
using TMPro;
using UnityEngine;

public class DebugInfo : MonoBehaviour
{
    public PlayerController playerControl;

    private StringBuilder m_SbCharacterState = new StringBuilder();
    private TextMeshProUGUI m_TextCharacterState;

    private void Awake()
    {
        m_TextCharacterState = transform.Find("CharacterStateInfo").GetComponent<TextMeshProUGUI>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playerControl == null)
            throw new System.Exception("PlayerController hasn't been assigned");
    }

    // Update is called once per frame
    void Update()
    {
        DrawCharacterState();
    }

    private void DrawCharacterState()
    {
        m_SbCharacterState.Clear();
        m_SbCharacterState.Append("player state: ")
            .Append(playerControl.currentState.GetType().Name);
        m_TextCharacterState.text = m_SbCharacterState.ToString();
    }
}
