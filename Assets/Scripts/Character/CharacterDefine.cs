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

public enum EFootstep : UInt16
{
    None = 0,
    LeftFootstep,
    RightFootstep,
}