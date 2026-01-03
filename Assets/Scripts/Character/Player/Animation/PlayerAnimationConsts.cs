using System;
using UnityEngine;

[Serializable]
public class PlayerAnimationConsts
{
    [SerializeField] private string idleParamName = "isIdling";
    [SerializeField] private string movingParamName = "isMoving";
    [SerializeField] private string walkParamName = "isWalking";
    [SerializeField] private string runParamName = "isRunning";

    public int idleHash { get; private set; }
    public int moveHash { get; private set; }
    public int walkHash { get; private set; }
    public int runHash { get; private set; }

    public void Init()
    {
        idleHash = Animator.StringToHash(idleParamName);
        moveHash = Animator.StringToHash(movingParamName);
        walkHash = Animator.StringToHash(walkParamName);
        runHash = Animator.StringToHash(runParamName);
    }
}
