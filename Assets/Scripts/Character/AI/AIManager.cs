using System.Collections.Generic;
using UnityEngine;

public class AIManager : Singleton<AIManager>
{
    private Dictionary<int, CharacterControllerBase> m_AIDict = new Dictionary<int, CharacterControllerBase>();

    public bool isEmpty => m_AIDict.Count == 0;
    public Dictionary<int, CharacterControllerBase>.Enumerator enumerator => m_AIDict.GetEnumerator();

    public override void OnDeInit()
    {
        m_AIDict.Clear();
    }

    public void Register(CharacterControllerBase character)
    {
        if(character == null) return;

        if (!m_AIDict.ContainsKey(character.gameObject.GetInstanceID()))
        {
            m_AIDict.Add(character.gameObject.GetInstanceID(), character);
        }
    }

    public void UnRegister(CharacterControllerBase character)
    {
        if (character == null) return;

        if (m_AIDict.ContainsKey(character.gameObject.GetInstanceID()))
        {
            m_AIDict.Remove(character.gameObject.GetInstanceID());
        }
    }
}
