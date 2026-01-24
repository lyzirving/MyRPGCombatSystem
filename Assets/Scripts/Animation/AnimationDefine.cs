using System;

public enum AnimationEventType : UInt32
{
    None = 0,
    // Events for locomotion
    Locomotion = 1,
    LeftFootStep,
    RightFootStep,
    JumpStart,
    JumpStartTransit,
    LandFinish,
    RollTransit,
    // Events for combat
    Combat = 100,
}
