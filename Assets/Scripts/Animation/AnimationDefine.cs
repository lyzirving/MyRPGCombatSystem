using System;

namespace AnimationDefine
{
    public enum PlayerAnimationEvent : UInt16
    {
        None = 0,
        LeftFootStep,
        RightFootStep,
        JumpStart,
        JumpTop,
        LandFinish
    }
}
