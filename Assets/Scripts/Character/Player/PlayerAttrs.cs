using System;
using UnityEngine;

[Serializable]
public class PlayerAttrs
{
    public PlayerState currentState = PlayerState.Idle;
    public Vector3 moveHorizonSpeed = Vector3.zero;
    public float yVelocity = 0f;
}
