using System;

[Serializable]
public class PlayerConfig
{
    public float gravity = -9.8f;
    public float walkSpeed = 2.5f;
    public float runSpeed = 5f;
    public float jumpMoveSpeed = 2f;
    public float jumpPower = 2f;
    public float fallGravityRatio = 1.3f;
    public float rotateSpeed = 8f;
}
