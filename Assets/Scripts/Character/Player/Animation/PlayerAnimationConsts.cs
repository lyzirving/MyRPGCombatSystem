using System;
using UnityEngine;

[Serializable]
public class PlayerAnimationConsts
{
    [SerializeField] private string groundParamName = "isGrounded";    
    [SerializeField] private string idleParamName = "isIdling";
    [SerializeField] private string movingParamName = "isMoving";    
    [SerializeField] private string walkParamName = "isWalking";
    [SerializeField] private string runParamName = "isRunning";
    [SerializeField] private string rollParamName = "isRolling";
    [SerializeField] private string landParamName = "isLand";

    [SerializeField] private string combatParamName = "isCombat";
    [SerializeField] private string lightAttackParamName = "isLightAttack";

    [SerializeField] private string airborneParamName = "isAirborne";
    [SerializeField] private string jumpStartLeftParamName = "isJumpStartLeft";
    [SerializeField] private string jumpStartRightParamName = "isJumpStartRight";
    [SerializeField] private string jumpIdleParamName = "isJumpIdle";
    [SerializeField] private string fallLeftParamName = "isFallingLeft";
    [SerializeField] private string fallRightParamName = "isFallingRight";          

    public int groundHash { get; private set; }    
    public int idleHash { get; private set; }
    public int moveHash { get; private set; }    
    public int walkHash { get; private set; }
    public int runHash { get; private set; }
    public int rollHash { get; private set; }
    public int landHash { get; private set; }

    public int combatHash { get; private set; }
    public int lightAttackHash { get; private set; }

    public int airborneHash { get; private set; }
    public int jumpStartLeftHash { get; private set; }
    public int jumpStartRightHash { get; private set; }
    public int jumpIdleHash { get; private set; }
    public int fallLeftHash { get; private set; }
    public int fallRightHash { get; private set; }

    public void Init()
    {
        groundHash = Animator.StringToHash(groundParamName);        
        idleHash = Animator.StringToHash(idleParamName);
        moveHash = Animator.StringToHash(movingParamName);        
        walkHash = Animator.StringToHash(walkParamName);
        runHash = Animator.StringToHash(runParamName);
        rollHash = Animator.StringToHash(rollParamName);
        landHash = Animator.StringToHash(landParamName);

        combatHash = Animator.StringToHash(combatParamName);
        lightAttackHash = Animator.StringToHash(lightAttackParamName);

        airborneHash = Animator.StringToHash(airborneParamName);
        jumpStartLeftHash = Animator.StringToHash(jumpStartLeftParamName);
        jumpStartRightHash = Animator.StringToHash(jumpStartRightParamName);
        jumpIdleHash = Animator.StringToHash(jumpIdleParamName);
        fallLeftHash = Animator.StringToHash(fallLeftParamName);
        fallRightHash = Animator.StringToHash(fallRightParamName);
    }
}
