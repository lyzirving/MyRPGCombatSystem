using System;
using System.Collections.Generic;

public class GameplayAttributeSet
{
    private Dictionary<EGameplayAttributeType, GameplayAttribute> m_AttributeDict = new Dictionary<EGameplayAttributeType, GameplayAttribute>();

    #region Attribute Operations
    public void Add(GameplayAttribute attribute)
    {
        m_AttributeDict[attribute.type] = attribute;
    }

    public GameplayAttribute Get(EGameplayAttributeType type)
    {
        m_AttributeDict.TryGetValue(type, out var attribute);
        return attribute;
    }

    public bool TryGet(EGameplayAttributeType type, out GameplayAttribute target)
    {
        return m_AttributeDict.TryGetValue(type, out target);
    }

    public void Clear()
    {
        m_AttributeDict.Clear();
    }
    #endregion

    #region Modifiers Operations
    public void ApplyEffect(GameplayEffect effect, Action<GameplayAttribute> onAttributeChanged = null)
    {
        if (effect == null || effect.modifiers == null)
            return;

        foreach (var item in effect.modifiers)
        {
            if (item.source == null)
                item.source = effect;

            ApplyModifier(item, onAttributeChanged);
        }
    }

    public void ApplyEffectImmediately(GameplayEffect effect, Action<GameplayAttribute> onAttributeChanged = null)
    {
        if (effect == null || effect.modifiers == null)
            return;

        foreach (var item in effect.modifiers)
        {
            if (item.source == null)
                item.source = effect;

            ApplyModifierImmediately(item, onAttributeChanged);
        }
    }

    private void ApplyModifier(AttributeModifier modifier, Action<GameplayAttribute> onAttributeChanged = null)
    {
        if (modifier == null || !TryGet(modifier.target, out var attribute))
            return;

        attribute.AddModifier(modifier);
        onAttributeChanged?.Invoke(attribute);
    }    

    private void ApplyModifierImmediately(AttributeModifier modifier, Action<GameplayAttribute> onAttributeChanged = null)
    {
        if(modifier == null || !TryGet(modifier.target, out var attribute))
            return;

        switch (modifier.modifierType)
        {
            case ModifierType.Add:
                attribute.baseValue = attribute.baseValue + modifier.value;
                break;
            case ModifierType.Multiply:
                attribute.baseValue = attribute.baseValue * (1 + modifier.value);
                break;
            case ModifierType.Override:
                attribute.baseValue = modifier.value;
                break;
        }
        onAttributeChanged?.Invoke(attribute);
    }

    public void RemoveEffectModifiers(GameplayEffect effect)
    {
        var modifiers = effect.modifiers;
        foreach (var mod in modifiers)
        {
            if (m_AttributeDict.TryGetValue(mod.target, out var targetAttribute))
                targetAttribute.RemoveAllModifiersFromSource(effect);
        }
    }

    public void ClearAllModifiers()
    {
        foreach (var item in m_AttributeDict)
        {
            item.Value.ClearAllModifiers();
        }
    }
    #endregion

    #region Cost Operations
    public bool CanPay(AbilityCost cost)
    {
        if (cost == null || !TryGet(cost.attribute, out var attribute))
            return false;

        return attribute.currentValue >= cost.value;
    }

    public void Pay(AbilityCost cost, Action<GameplayAttribute> onAttributeChanged = null)
    {
        if (cost == null || !TryGet(cost.attribute, out var attribute))
            return;

        var mod = new AttributeModifier(cost.attribute, ModifierType.Add, -cost.value, 0, cost);
        ApplyModifierImmediately(mod, onAttributeChanged);
    }
    #endregion
}
