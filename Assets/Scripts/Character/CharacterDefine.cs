using System;

public enum ECharacterState : UInt16
{
    Idle = 0,
    Move,
    Jump,
    Roll,
    Falling,
    Land,
    Attack
}

public enum EFootStep : UInt16
{
    None = 0,
    LeftFootStep,
    RightFootStep,
}