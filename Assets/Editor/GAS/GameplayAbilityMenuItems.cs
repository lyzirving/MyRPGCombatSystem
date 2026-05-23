using UnityEditor;

public class GameplayAbilityMenuItems
{
    [MenuItem("Assets/Create/GAS/Ability/GameplayAbility", priority = 10)]
    private static void CreateGameplayAbility() => GameplayAbilityCreator.CreateGameplayAbility<GameplayAbility>();

    [MenuItem("Assets/Create/GAS/Ability/LightAttackGameplayAbility", priority = 10)]
    private static void CreateLightAttackGameplayAbility() => GameplayAbilityCreator.CreateGameplayAbility<LightAttackGameplayAbility>();
}
