using System.Text;
using TMPro;
using UnityEngine;

public class DebugInfo : MonoBehaviour
{
    public PlayerController playerControl;

    private StringBuilder m_SbCharacterState = new StringBuilder();
    private StringBuilder m_SbAbilityInfo = new StringBuilder();
    private StringBuilder m_SbTagsInfo = new StringBuilder();

    private TextMeshProUGUI m_TextCharacterState;
    private TextMeshProUGUI m_TextActiveAbilityInfo;
    private TextMeshProUGUI m_TextActiveTagsInfo;

    private void Awake()
    {
        m_TextCharacterState = transform.Find("CharacterStateInfo").GetComponent<TextMeshProUGUI>();
        m_TextActiveAbilityInfo = transform.Find("ActiveAbilityInfo").GetComponent<TextMeshProUGUI>();
        m_TextActiveTagsInfo = transform.Find("ActiveTagsInfo").GetComponent<TextMeshProUGUI>();
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
        DrawActiveTagsInfo();
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

    private void DrawActiveTagsInfo()
    {
        bool hasValue = false;
        m_SbTagsInfo.Clear();
        m_SbTagsInfo.Append("active tags: ");
        var indices = playerControl.abilitySystemComp.activeTagIndice;
        if (indices != null)
        {
            foreach (var tagIdx in indices)
            {
                if(hasValue)
                    m_SbTagsInfo.Append("\n");
                var name = GameplayTagManager.instance.GetName(tagIdx);
                m_SbTagsInfo.Append($"{name}");
                hasValue = true;
            }
        }
        m_TextActiveTagsInfo.text = m_SbTagsInfo.ToString();
    }
}
