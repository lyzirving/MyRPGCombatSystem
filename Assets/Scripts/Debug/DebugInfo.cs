using System;
using System.Text;
using TMPro;
using UnityEngine;

public class DebugInfo : MonoBehaviour
{
    public PlayerController playerControl;

    private StringBuilder m_SbCharacterState = new StringBuilder();
    private StringBuilder m_SbAbilityInfo = new StringBuilder();

    private TextMeshProUGUI m_TextCharacterState;
    private TextMeshProUGUI m_TextActiveAbilityInfo;

    private void Awake()
    {
        m_TextCharacterState = transform.Find("CharacterStateInfo").GetComponent<TextMeshProUGUI>();
        m_TextActiveAbilityInfo = transform.Find("ActiveAbilityInfo").GetComponent<TextMeshProUGUI>();
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
        DrawActiveAbilityInfo();
    }    

    private void DrawCharacterState()
    {
        m_SbCharacterState.Clear();
        m_SbCharacterState.Append("player state: ")
            .Append(playerControl.currentState.GetType().Name);
        m_TextCharacterState.text = m_SbCharacterState.ToString();
    }

    private void DrawActiveAbilityInfo()
    {
        bool hasValue = false;
        m_SbAbilityInfo.Clear();
        m_SbAbilityInfo.Append("active ability: ");        
        foreach (var ability in playerControl.abilitySystemComp.activeAbilities.Values)
        {
            if (hasValue)
                m_SbAbilityInfo.Append(", ");
            m_SbAbilityInfo.Append($"{ability.abilityName}");
            hasValue = true;
        }
        m_TextActiveAbilityInfo.text = m_SbAbilityInfo.ToString();
    }
}
