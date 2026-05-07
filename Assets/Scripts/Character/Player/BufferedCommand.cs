using System;
using UnityEngine;

public struct BufferedCommand : IComparable<BufferedCommand>
{    
    public ECharacterAction action;
    public int priority;
    public float inputTime;
    public float duration;

    public BufferedCommand(ECharacterAction action, int priority = 0, float duration = 0.5f)
    {
        this.action = action;
        this.priority = priority;
        this.inputTime = Time.time;
        this.duration = duration;
    }

    public int CompareTo(BufferedCommand other)
    {
        if (this.priority > other.priority)
            return 1;
        else if (this.priority == other.priority)
            return 0;
        else
            return -1;
    }

    public bool IsValid()
    {
        return Time.time - inputTime <= duration;
    }

    public static int Priority(ECharacterAction type)
    {
        switch (type)
        {
            case ECharacterAction.LightAttack: return 10;
            case ECharacterAction.Defence:     return 5;
            default: return 0;  
        }
    }
}
