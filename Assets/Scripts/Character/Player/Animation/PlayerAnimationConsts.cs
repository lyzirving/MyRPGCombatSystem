using System;
using UnityEngine;

[Serializable]
public class PlayerAnimationConsts
{
    [SerializeField] private string idleParamName = "isIdling";

    public int idleHash { get; private set; }

    public void Init()
    {
        idleHash = Animator.StringToHash(idleParamName);
    }
}
