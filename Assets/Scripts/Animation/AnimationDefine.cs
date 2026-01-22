using System;

namespace AnimationDefine
{
    public enum PlayerAnimationEvent : UInt16
    {
        None = 0,
        LeftFootStep,
        RightFootStep,
        JumpStart,
        JumpStartTransit,
        LandFinish,
        RollTransit,
    }    
}

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
