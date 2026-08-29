using UnityEditor;

public class GameplayAbilityMenuItems
{
    [MenuItem("Assets/Create/GAS/Ability/AttackAbility", priority = 10)]
    private static void CreateAttackAbility() => GameplayAbilityCreator.CreateGameplayAbility<AttackAbility>();

    [MenuItem("Assets/Create/GAS/Ability/AirborneAttackAbility", priority = 10)]
    private static void CreateAirborneAttackAbility() => GameplayAbilityCreator.CreateGameplayAbility<AirborneAttackAbility>();

    [MenuItem("Assets/Create/GAS/Ability/JumpAbility", priority = 10)]
    private static void CreateJumpAbility() => GameplayAbilityCreator.CreateGameplayAbility<JumpAbility>();

    [MenuItem("Assets/Create/GAS/Ability/DodgeAbility", priority = 10)]
    private static void CreateDodgeAbility() => GameplayAbilityCreator.CreateGameplayAbility<DodgeAbility>();

    [MenuItem("Assets/Create/GAS/Ability/DefenceAbility", priority = 10)]
    private static void CreateDefenceAbility() => GameplayAbilityCreator.CreateGameplayAbility<DefenceAbility>();

    [MenuItem("Assets/Create/GAS/Ability/LockTargetAbility", priority = 10)]
    private static void CreateLockTargetAbility() => GameplayAbilityCreator.CreateGameplayAbility<LockTargetAbility>();
    
    [MenuItem("Assets/Create/GAS/Ability/LocomotionAbility", priority = 10)]
    private static void CreateLocomotionAbility() => GameplayAbilityCreator.CreateGameplayAbility<LocomotionAbility>();
}
