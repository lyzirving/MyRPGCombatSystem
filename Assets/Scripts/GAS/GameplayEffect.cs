using System;
using System.Collections.Generic;
using UnityEngine;

public enum EffectDurationType
{
    Instant, 
    Duration,
    Periodical
}

[CreateAssetMenu(fileName = "NewGameplayEffect", menuName = "GAS/GameplayEffect")]
public class GameplayEffect : ScriptableObject
{
    [Header("Basic")]
    public string effectName;
    [TextArea] public string description;
    public List<AttributeModifier> modifiers = new List<AttributeModifier>();
    public EffectDurationType durationType = EffectDurationType.Instant;
    public float duration = 0f;
       
    [Header("Tags")]
    public List<GameplayTag> grantedTags = new List<GameplayTag>();
    public List<GameplayTag> removedTags = new List<GameplayTag>();
    public List<GameplayTag> requiredTags = new List<GameplayTag>();
    public List<GameplayTag> blockedTags = new List<GameplayTag>();

    public bool isInstant => durationType == EffectDurationType.Instant;

    public virtual bool CanApplyTo(AbilitySystemComponent targetASC)
    {
        // Check whether target has any block tag
        if (targetASC.HasAnyTag(blockedTags))
            return false;

        if (requiredTags.Count > 0 && !targetASC.HasAllTags(requiredTags))
            return false;

        return true;
    }
}
