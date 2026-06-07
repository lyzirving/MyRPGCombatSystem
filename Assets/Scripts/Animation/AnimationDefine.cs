using System;
using UnityEngine;

public enum AnimationEventType : UInt32
{
    None = 0,
    // Events for locomotion
    Locomotion = 1,
    // Events for combat
    Combat = 100,
    AttackStart,
    AttackEnd,
    AttackCombo,
    AttackVfx,
    // Events for common animation
    Common = 500,
    AnimationStart,
    AnimationTransit,    
}

public static class AnimationConsts
{
    public const int BASE_LAYER = 0;
    public const int HURT_LAYER = 1;

    [Header("Transition parameters in Animator")]
    public static int locomotion { get; private set; }
    public static int locked { get; private set; }
    public static int combat { get; private set; }
    public static int land { get; private set; }
    public static int airborne { get; private set; }
    public static int hit { get; private set; }
    public static int dodge { get; private set; }
    public static int strafe { get; private set; }
    public static int defence { get; private set; }
    public static int defenceRelease { get; private set; }
    public static int roar { get; private set; }

    [Header("Ratio parameters in Animator")]
    public static int angular { get; private set; }
    public static int verticalAngular { get; private set; }
    public static int speed { get; private set; }
    public static int jumpRatio { get; private set; }
    public static int feetTween { get; private set; }
    public static int hitTween { get; private set; }
    public static int hitStunning { get; private set; }

    [Header("Animation state in Animator")]
    public static int hurtState { get; private set; }
    public static int defenceState { get; private set; }    

    public static void Init()
    {
        // Transition parameters in Animator
        locomotion = Animator.StringToHash("isLocomotion");
        locked = Animator.StringToHash("isLocked");
        combat = Animator.StringToHash("isCombat");
        land = Animator.StringToHash("isLanding");
        airborne = Animator.StringToHash("isAirborne");
        hit = Animator.StringToHash("isHit");
        dodge = Animator.StringToHash("isDodge");
        strafe = Animator.StringToHash("isStrafe");
        defence = Animator.StringToHash("isDefence");
        defenceRelease = Animator.StringToHash("defenceRelease");
        roar = Animator.StringToHash("roar");

        // Ratio parameters in Animator
        angular = Animator.StringToHash("angular");
        verticalAngular = Animator.StringToHash("verticalAngular");
        speed = Animator.StringToHash("speed");
        jumpRatio = Animator.StringToHash("jumpRatio");
        feetTween = Animator.StringToHash("feetTween");
        hitTween = Animator.StringToHash("hitTween");
        hitStunning = Animator.StringToHash("hitStunning");

        // Animation state in Animator
        hurtState = Animator.StringToHash("Hurt");
        defenceState = Animator.StringToHash("DefenceStart");
    }
}

