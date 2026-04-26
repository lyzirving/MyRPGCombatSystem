using System;

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

public enum ECharacterDodgeAction : UInt16
{
    None = 0,
    Forward,
    Backward,
    Left,
    Right
}