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
    // Events for common animation
    Common = 500,
    AnimationStart,
    AnimationTransit,    
}

public static class AnimationConsts
{
    public const int BASE_LAYER = 0;
    public const int HURT_LAYER = 1;

    // Animation parameter name in Animator
    private const string combatName = "isCombat";
    private const string isLandName = "isLanding";
    private const string airborneName = "isAirborne";
    private const string hitName = "isHit";    

    private const string angularName = "angular";
    private const string speedName = "speed";
    private const string jumpRatioName = "jumpRatio";
    private const string feetTweenName = "feetTween";

    private const string hitTweenName = "hitTween";
    private const string hitRatioName = "hitRatio";

    // Animation state name in Animator
    private const string hurtName = "Hurt";

    public static int combat { get; private set; }
    public static int land { get; private set; }
    public static int airborne { get; private set; }
    public static int hit { get; private set; }   

    public static int angular { get; private set; }
    public static int speed { get; private set; }
    public static int jumpRatio { get; private set; }
    public static int feetTween { get; private set; }

    public static int hitTween { get; private set; }
    public static int hitRatio { get; private set; }

    public static int hurt { get; private set; }

    public static void Init()
    {        
        combat = Animator.StringToHash(combatName);
        land = Animator.StringToHash(isLandName);
        airborne = Animator.StringToHash(airborneName);
        hit = Animator.StringToHash(hitName);
        hurt = Animator.StringToHash(hurtName);

        angular = Animator.StringToHash(angularName);
        speed = Animator.StringToHash(speedName);
        jumpRatio = Animator.StringToHash(jumpRatioName);
        feetTween = Animator.StringToHash(feetTweenName);

        hitTween = Animator.StringToHash(hitTweenName);
        hitRatio = Animator.StringToHash(hitRatioName);
    }
}

