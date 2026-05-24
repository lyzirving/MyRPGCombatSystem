using System;
using System.Collections.Generic;

public enum ModifierType
{
    Add,     
    Multiply,
    Override 
}

[Serializable]
public class AttributeModifier
{
    public EGameplayAttributeType target;
    public ModifierType modifierType;
    public float value;    
    /// <summary>
    /// Modifier is sorted by priority in Ascending order.
    /// eg: if mod1's priority is 1, and mod2's priority is 2, mod2 will be calculated later.
    /// </summary>
    public int priority;
    public object source;

    public AttributeModifier(EGameplayAttributeType target, ModifierType type, float value, int priority = 0, object source = null)
    {
        this.target = target;
        this.modifierType = type;
        this.value = value;        
        this.priority = type != ModifierType.Override ? priority : int.MaxValue;
        this.source = source;
    }

    public static void Sort(List<AttributeModifier> modifiers)
    {
        if (modifiers != null && modifiers.Count > 1)
        {
            modifiers.Sort((a, b) => a.priority.CompareTo(b.priority));
        }
    }
}