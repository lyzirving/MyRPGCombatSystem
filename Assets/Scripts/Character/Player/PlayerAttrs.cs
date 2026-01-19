using System;
using UnityEngine;

[Serializable]
public class PlayerAttrs
{
    public EPlayerState currentState = EPlayerState.Idle;

    public float speedModify = 0f;
    public Vector3 jumpForce = Vector3.zero;
}
