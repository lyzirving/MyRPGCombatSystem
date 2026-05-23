using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameplayAbilitySet", menuName = "GAS/GameplayAbilitySet")]
public class GameplayAbilitySet : ScriptableObject
{
    [SerializeField] private List<GameplayAbility> m_AbilityList = new List<GameplayAbility>();
    private Dictionary<string, GameplayAbility> m_AbilityMap = new Dictionary<string, GameplayAbility>();

    public List<GameplayAbility> abilities => m_AbilityList;

    private void OnEnable()
    {
        SyncAbilityMap();
    }

    private void SyncAbilityMap()
    {        
        m_AbilityMap.Clear();
        for (int i = 0; i < m_AbilityList.Count; ++i)
        {
            var ability = m_AbilityList[i];
            if(ability == null)
                continue;

            if (m_AbilityMap.ContainsKey(ability.guid))
            {
                var exist = m_AbilityMap[ability.guid];
                if (exist != ability)
                    throw new System.Exception($"SyncAbilityMap Error! The same guid[{ability.guid}] matches two different ability, one is [{ability}], another is [{exist}] ");
            }
            else
                m_AbilityMap.Add(ability.guid, ability);
        }
    }

    public bool Add(GameplayAbility ability)
    { 
        if (ability == null)
            return false;

        if (m_AbilityMap.ContainsKey(ability.guid))
        {
            var exist = m_AbilityMap[ability.guid];
            if (exist != ability)
                throw new System.Exception($"SyncAbilityMap Error! The same guid[{ability.guid}] matches two different ability, one is [{ability}], another is [{exist}] ");
            return true;
        }
        else
        {
            m_AbilityMap.Add(ability.guid, ability);
            m_AbilityList.Add(ability);
            return true;
        }
    }

    public bool Remove(GameplayAbility ability)
    {
        if (ability == null)
            return false;

        if (m_AbilityMap.ContainsKey(ability.guid))
        {
            m_AbilityList.Remove(ability);
            m_AbilityMap.Remove(ability.guid);
            return true;
        }
        return false;
    }
}
