using System;

[Serializable]
public class PlayerConfig
{
    public float gravity = -9.8f;
    public float walkSpeed = 2.5f;
    public float runSpeed = 5f;
    public float jumpMoveSpeed = 2f;
    public float jumpRatio = 2f;
    public float floatingRatio = 0.35f;
    public float rotateSpeed = 8f;
}
