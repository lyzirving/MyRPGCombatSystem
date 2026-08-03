using System;

public enum ECharacterAction : UInt16
{
    None = 0,
    Idle,
    Move,
    Sprint,
    Jump,
    Chase,
    Attack,
    LightAttack, 
    HeavyAttack,   
    CounterAttack,
    Defence,
    Dodge,
    Count
}

public enum ECharacterDodgeAction : UInt16
{
    None = 0,
    Forward,
    Backward,
    Left,
    Right
}