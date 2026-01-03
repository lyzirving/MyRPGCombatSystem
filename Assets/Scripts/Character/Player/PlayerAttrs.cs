using System;

[Serializable]
public class PlayerAttrs
{
    public PlayerState currentState = PlayerState.Idle;
    public float yVelocity = 0f;
}
