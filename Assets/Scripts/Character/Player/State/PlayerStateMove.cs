using UnityEngine;

public class PlayerStateMove : PlayerStateBase
{
    private Vector3 m_Velocity = Vector3.zero;

    public override void Enter()
    {
        m_Player.model.StartAnimation(m_Player.animConsts.moveHash);
    }

    public override void Exit()
    {
        m_Player.model.StopAnimation(m_Player.animConsts.moveHash);
    }

    public override void Update()
    {
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
    protected void Move()
    {
        // Slight downward velocity to keep grounded stable
        if (m_Velocity.y < -2f)
            m_Velocity.y = -2f;

        Vector2 input = InputManager.instance.playerMovement;
        Vector3 move = new Vector3(input.x, 0, input.y);
        move = Vector3.ClampMagnitude(move, 1f);

        // deal rotation from camera
        Vector3 euler = new Vector3(0f, Camera.main.transform.eulerAngles.y, 0f);
        Vector3 targetDir = Quaternion.Euler(euler) * move;
        m_Player.transform.rotation = Quaternion.Slerp(m_Player.transform.rotation,
            Quaternion.LookRotation(targetDir), Time.deltaTime *  m_Player.config.rotateSpeed);

        // Apply gravity
        m_Velocity.y += m_Player.config.gravity * Time.deltaTime;

        // Move
        Vector3 finalMove = targetDir * m_Player.config.walkSpeed + Vector3.up * m_Velocity.y;
        m_Player.character.Move(finalMove * Time.deltaTime);
    }
}
