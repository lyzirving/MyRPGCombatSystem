
using System;

public enum EPlayerState : UInt16
{
    Idle = 0,
    Walk,
    Run,
    Jump,
    JumpIdle,
    Roll,
    Falling,
    Land,
    StandardAttack
}

public enum EFootStep : UInt16
{
    None = 0,
    LeftFootStep,
    RightFootStep,
}
