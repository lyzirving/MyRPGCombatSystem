using System;

public enum ECharacterState : UInt16
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
public enum EDefenceState : UInt16
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

public enum EAttackState : UInt16
{
    Start = 0,    
    ReadyCombo,
    End,
    Num
}

public enum EDodgeState : UInt16
{
    Start = 0,
    Floating,
    Stop
}

public enum EJumpState : UInt16
{
    Start = 0,
    Airborne,
    Landed
}

public enum EFootstep : UInt16
{
    None = 0,
    LeftFootstep,
    RightFootstep,
}