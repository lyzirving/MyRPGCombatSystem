using System;
using UnityEngine;

[Serializable]
public class PlayerAttrs
{
    public EPlayerState currentState = EPlayerState.Idle;
    public float yVelocity = 0f;
}
