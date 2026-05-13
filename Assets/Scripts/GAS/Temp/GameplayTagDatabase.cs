using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameplayTagDatabase", menuName = "GAS/GameplayTagDatabase")]
public class GameplayTagDatabase : ScriptableObject
{
    public List<GameplayTag2> allTags = new List<GameplayTag2>();
}