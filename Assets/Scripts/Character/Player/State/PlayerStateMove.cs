using UnityEngine;

public class PlayerStateMove : PlayerStateBase
{
    public override void Enter(StateBase exitState, in ChangeStateArgs args)
    {
        m_Player.model.StartAnimation(m_Player.animConsts.moveHash);
    }

    public override void Exit(StateBase newState)
    {
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(PlayerStateMove)))
        {
            m_Player.model.StopAnimation(m_Player.animConsts.moveHash);
        }
    }

    public override void Update()
    {
        if (InputManager.instance.isPlayerJumpPerformed)
        {
            m_Player.ChangeState(PlayerState.Jump);
            return;
        }

        if (!InputManager.instance.isPlayerMoving)
        {
            m_Player.ChangeState(PlayerState.Idle);
            return;
        }

        Move();
    }

    /// <summary>
    /// Move function reference to Unity's ScriptReference/CharacterController.Move.html
    /// </summary>
    protected void Move(bool useGravity = true)
    {
        // Slight downward velocity to keep grounded stable
        if (m_Player.attrs.yVelocity < -2f)
            m_Player.attrs.yVelocity = -2f;

        Vector2 input = InputManager.instance.playerMovement;
        Vector3 move = new Vector3(input.x, 0, input.y);
        move = Vector3.ClampMagnitude(move, 1f);

        // deal rotation from camera
        Vector3 euler = new Vector3(0f, Camera.main.transform.eulerAngles.y, 0f);
        Vector3 targetDir = Quaternion.Euler(euler) * move;
        m_Player.transform.rotation = Quaternion.Slerp(m_Player.transform.rotation,
            Quaternion.LookRotation(targetDir), Time.deltaTime *  m_Player.config.rotateSpeed);

        // Apply gravity
        m_Player.attrs.yVelocity += m_Player.config.gravity * Time.deltaTime;

        // Move
        Vector3 finalMove = targetDir * m_Player.GetMoveSpeed() + Vector3.up * m_Player.attrs.yVelocity * (useGravity ? 1f : 0f);
        m_Player.character.Move(finalMove * Time.deltaTime);

        m_Player.attrs.moveHorizonSpeed.x = m_Player.character.velocity.x;
        m_Player.attrs.moveHorizonSpeed.y = 0f;
        m_Player.attrs.moveHorizonSpeed.z = m_Player.character.velocity.z;
    }
}
