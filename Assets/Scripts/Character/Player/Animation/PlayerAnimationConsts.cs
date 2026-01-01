using System;
using UnityEngine;

[Serializable]
public class PlayerAnimationConsts
{
    [SerializeField] private string idleParamName = "isIdling";
    [SerializeField] private string movingParamName = "isMoving";

    public int idleHash { get; private set; }
    public int moveHash { get; private set; }

    public void Init()
    {
        idleHash = Animator.StringToHash(idleParamName);
        moveHash = Animator.StringToHash(movingParamName);
    }
}
