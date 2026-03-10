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
    Hurt
}

public enum EFootstep : UInt16
{
    None = 0,
    LeftFootstep,
    RightFootstep,
}