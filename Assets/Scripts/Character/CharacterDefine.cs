using System;

public enum ECharacterState : UInt16
{
    Idle = 0,
    Move,
    Jump,
    Roll,
    Falling,
    Land,
    Attack,
    Hurt,
    Defence,
    Roar,
    Num
}

public enum ECharacterAction : UInt16
{ 
    None = 0,
    Idle,
    Move,
    Jump,
    Chase,
    Attack,
    CounterAttack,
    Defence,
    Dodge,    
    Count
}

/// <summary>
/// Internal state of defence within a character
/// </summary>
public enum EDefenceState : UInt16
{
    Enter = 0,
    Loop,
    CounterAttackAWait,
    CounterAttackPerform,
    CounterAttackRunOut,
    End
}

public enum EFootstep : UInt16
{
    None = 0,
    LeftFootstep,
    RightFootstep,
}