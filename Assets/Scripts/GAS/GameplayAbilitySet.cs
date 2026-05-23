using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameplayAbilitySet", menuName = "GAS/GameplayAbilitySet")]
public class GameplayAbilitySet : ScriptableObject, IReadOnlyList<GameplayAbility>
{
    [SerializeField] private List<GameplayAbility> m_AbilityList = new List<GameplayAbility>();
    private Dictionary<int, GameplayAbility> m_AbilityMap = new Dictionary<int, GameplayAbility>();

    public int Count => m_AbilityList.Count;

    public IEnumerator<GameplayAbility> GetEnumerator() => m_AbilityList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public GameplayAbility this[int index] => m_AbilityList[index];

    public bool Has<T>() where T : GameplayAbility => m_AbilityMap.ContainsKey(AbilityHash<T>.classHash);

    public GameplayAbility Get<T>() where T : GameplayAbility => m_AbilityMap[AbilityHash<T>.classHash];

    private void OnEnable()
    {
        SyncAbilityMap();
    }    

    public bool Add(GameplayAbility ability)
    { 
        if (ability == null)
            return false;

        if (m_AbilityMap.ContainsKey(ability.classHash))
        {
            var exist = m_AbilityMap[ability.classHash];
            if (exist != ability)
                throw new System.Exception($"SyncAbilityMap Error! The same guid[{ability.guid}] matches two different ability, one is [{ability}], another is [{exist}] ");
            return true;
        }
        else
        {
            m_AbilityMap.Add(ability.classHash, ability);
            m_AbilityList.Add(ability);
            return true;
        }
    }

    public bool Remove(GameplayAbility ability)
    {
        if (ability == null)
            return false;

        if (m_AbilityMap.ContainsKey(ability.classHash))
        {
            m_AbilityList.Remove(ability);
            m_AbilityMap.Remove(ability.classHash);
            return true;
        }
        return false;
    }    

    private void SyncAbilityMap()
    {
        m_AbilityMap.Clear();
        for (int i = 0; i < m_AbilityList.Count; ++i)
        {
            var ability = m_AbilityList[i];
            if (ability == null)
                continue;

            if (m_AbilityMap.ContainsKey(ability.classHash))
            {
                var exist = m_AbilityMap[ability.classHash];
                if (exist != ability)
                    throw new System.Exception($"SyncAbilityMap Error! The same guid[{ability.guid}] matches two different ability, one is [{ability}], another is [{exist}] ");
            }
            else
                m_AbilityMap.Add(ability.classHash, ability);
        }
    }    
}
