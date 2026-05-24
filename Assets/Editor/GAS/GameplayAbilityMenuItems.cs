using UnityEditor;

public class GameplayAbilityMenuItems
{
    [MenuItem("Assets/Create/GAS/Ability/LightAttackAbility", priority = 10)]
    private static void CreateLightAttackAbility() => GameplayAbilityCreator.CreateGameplayAbility<LightAttackAbility>();

    [MenuItem("Assets/Create/GAS/Ability/JumpAbility", priority = 10)]
    private static void CreateJumpAbility() => GameplayAbilityCreator.CreateGameplayAbility<JumpAbility>();

    [MenuItem("Assets/Create/GAS/Ability/DodgeAbility", priority = 10)]
    private static void CreateDodgeAbility() => GameplayAbilityCreator.CreateGameplayAbility<DodgeAbility>();

    [MenuItem("Assets/Create/GAS/Ability/DefenceAbility", priority = 10)]
    private static void CreateDefenceAbility() => GameplayAbilityCreator.CreateGameplayAbility<DefenceAbility>();
}
