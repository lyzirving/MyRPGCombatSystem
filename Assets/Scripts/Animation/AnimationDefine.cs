using System;
using UnityEngine;

public enum AnimationEventType : UInt32
{
    None = 0,
    // Events for locomotion
    Locomotion = 1,
    LeftFootStep,
    RightFootStep,
    // Events for combat
    Combat = 100,
    AttackStart,
    AttackEnd,
    AttackCombo,
    // Events for common animation
    Common = 500,
    AnimationStart,
    AnimationTransit,    
}

public static class AnimationConsts
{
    private const string combatName = "isCombat";
    private const string isLandName = "isLanding";
    private const string airborneName = "isAirborne";

    private const string angularName = "angular";
    private const string speedName = "speed";
    private const string jumpRatioName = "jumpRatio";
    private const string feetTweenName = "feetTween";

   

    public static int combat { get; private set; }
    public static int land { get; private set; }
    public static int airborne { get; private set; }


    public static int angular { get; private set; }
    public static int speed { get; private set; }
    public static int jumpRatio { get; private set; }
    public static int feetTween { get; private set; }

    public static void Init()
    {        
        combat = Animator.StringToHash(combatName);
        land = Animator.StringToHash(isLandName);
        airborne = Animator.StringToHash(airborneName);

        angular = Animator.StringToHash(angularName);
        speed = Animator.StringToHash(speedName);
        jumpRatio = Animator.StringToHash(jumpRatioName);
        feetTween = Animator.StringToHash(feetTweenName);
    }
}

