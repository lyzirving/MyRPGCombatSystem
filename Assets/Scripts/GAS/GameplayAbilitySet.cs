using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameplayAbilitySet", menuName = "GAS/GameplayAbilitySet")]
public class GameplayAbilitySet : ScriptableObject
{
    private Dictionary<int, GameplayAbility> m_AbilityMap = new Dictionary<int, GameplayAbility>();
    public Dictionary<int, GameplayAbility> abilities => m_AbilityMap;
}
