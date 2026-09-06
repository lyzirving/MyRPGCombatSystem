using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameplayTagDatabase", menuName = "GAS/GameplayTagDatabase")]
public class GameplayTagDatabase : ScriptableObject
{    
    [SerializeField] private List<GameplayTag> m_Tags = new List<GameplayTag>();

    public List<GameplayTag> tags => m_Tags;

    public bool AddTag(string name)
    {
        if (GameplayTagManager.instance.AddTag(name))
        {
            m_Tags.Add(GameplayTagManager.instance.GetTag(name));
            return true;
        }
        return false;
    }

    public bool RemoveTag(string name)
    {
        RemoveIterator(name);
        return GameplayTagManager.instance.RemoveTag(name);
    }

    public bool ChangeTagName(string oldName, string newName)
    {
        if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
            return false;

        if (GameplayTagManager.instance.ChangeTagName(oldName, newName))
        {
            for (int i = 0; i < m_Tags.Count; ++i)
            {
                if (m_Tags[i].name == oldName)
                    m_Tags.RemoveAt(i);
            }
            var newTag = GameplayTagManager.instance.GetTag(newName);
            m_Tags.Add(newTag);
            return true;
        }

        return false;
    }

    private void RemoveIterator(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        int idx = GameplayTagManager.instance.GetIndex(name);
        if(idx <= 0)
            return;

        GameplayTag tag = GameplayTagManager.instance.GetTag(name);
        if(!m_Tags.Remove(tag))   
            return;
        
        int[] childIndice = GameplayTagManager.instance.GetChildIndices(idx);
        if (childIndice != null && childIndice.Length > 0)
        {
            foreach (int i in childIndice)
                RemoveIterator(GameplayTagManager.instance.GetName(i));
        }
    }
}