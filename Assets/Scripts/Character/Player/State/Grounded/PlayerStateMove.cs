using UnityEngine;

public class PlayerStateMove : PlayerStateGrounded
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
        if (!InputManager.instance.isPlayerMoving)
        {
            m_Player.ChangeState(EPlayerState.Idle);
            return;
        }

        if (InputManager.instance.isPlayerJumpPerformed)
        {
            m_Player.ChangeState(EPlayerState.Jump);
            return;
        }
    }

    public override void FxiedUpdate()
    {
        Float();

        Move();
    }

    protected void Move()
    {
        if (!InputManager.instance.isPlayerMoving)
            return;

        Vector3 move = Vector3.zero;
        Vector2 input = InputManager.instance.playerMovement;
        move.x = input.x;
        move.z = input.y;
        move = Vector3.ClampMagnitude(move, 1f);

        // deal rotation from camera
        Vector3 euler = new Vector3(0f, Camera.main.transform.eulerAngles.y, 0f);
        Vector3 targetDir = Quaternion.Euler(euler) * move;

        m_Player.transform.rotation = Quaternion.Slerp(m_Player.transform.rotation,
            Quaternion.LookRotation(targetDir), Time.deltaTime * m_Player.config.rotateSpeed);
    
        m_Player.rigidBody.AddForce(m_Player.GetMoveSpeed() * targetDir - playerHorizonVelocity, ForceMode.VelocityChange);
    }
}
