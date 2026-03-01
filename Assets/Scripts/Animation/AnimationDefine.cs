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
    public const string hitLayer = "HitLayer";

    private const string isGroundName = "isGrounded";
    private const string isIdleName = "isIdling";
    private const string isMovingName = "isMoving";
    private const string isWalkName = "isWalking";
    private const string isRunName = "isRunning";
    private const string isRollName = "isRolling";
    private const string isLandName = "isLand";

    private const string combatName = "isCombat";
    private const string hitName = "isHit";

    private const string airborneName = "isAirborne";
    private const string fallLeftName = "isFallingLeft";
    private const string fallRightName = "isFallingRight";

    private const string jumpRatioName = "jumpRatio";
    private const string feetTweenName = "feetTween";

    public static int ground { get; private set; }
    public static int idle { get; private set; }
    public static int move { get; private set; }
    public static int walk { get; private set; }
    public static int run { get; private set; }
    public static int roll { get; private set; }
    public static int land { get; private set; }

    public static int combat { get; private set; }
    public static int hit { get; private set; }

    public static int airborne { get; private set; }
    public static int fallLeft { get; private set; }
    public static int fallRight { get; private set; }

    public static int jumpRatio { get; private set; }
    public static int feetTween { get; private set; }

    public static void Init()
    {
        ground = Animator.StringToHash(isGroundName);
        idle = Animator.StringToHash(isIdleName);
        move = Animator.StringToHash(isMovingName);
        walk = Animator.StringToHash(isWalkName);
        run = Animator.StringToHash(isRunName);
        roll = Animator.StringToHash(isRollName);
        land = Animator.StringToHash(isLandName);

        combat = Animator.StringToHash(combatName);
        hit = Animator.StringToHash(hitName);

        airborne = Animator.StringToHash(airborneName);
        fallLeft = Animator.StringToHash(fallLeftName);
        fallRight = Animator.StringToHash(fallRightName);

        jumpRatio = Animator.StringToHash(jumpRatioName);
        feetTween = Animator.StringToHash(feetTweenName);
    }
}

