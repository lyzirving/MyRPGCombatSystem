using System;

public enum ECharacterState : ushort
{
    Idle = 0,
    Move,
    StrafeMove,
    Sprint,
    Jump,
    Roll,
    Falling,
    Land,
    Attack,
    Hurt,
    Defence,
    Dodge,
    Roar,
    Num
}

/// <summary>
/// Internal state of defence within a character
/// </summary>
public enum EDefenceState : ushort
{
    Enter = 0,
    Loop,
    CounterAttackAWait,
    CounterAttackPerform,
    CounterAttackRunOut,
    Exiting,
    End,
    EndAndTransit
}

public enum EAttackState : ushort
{
    Start = 0,    
    ReadyCombo,
    End,
    Num
}

public enum EDodgeState : ushort
{
    Start = 0,
    Floating,
    Stop
}

public enum EJumpState : ushort
{
    Start = 0,
    Airborne,
    DoubleJump,
    Landed
}

public enum EFootstep : ushort
{
    None = 0,
    LeftFootstep,
    RightFootstep,
}