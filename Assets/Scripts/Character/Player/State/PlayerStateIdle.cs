using UnityEngine;

public class PlayerStateIdle : PlayerStateBase
{
    public override void Enter(StateBase exitState)
    {
        m_Player.model.StartAnimation(m_Player.animConsts.idleHash);
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.StopAnimation(m_Player.animConsts.idleHash);
    }

    public override void Update()
    {
        ApplyGravity();

        if (InputManager.instance.isPlayerMoving)
        {
            m_Player.ChangeState(InputManager.instance.shouldPlayerRun ? PlayerState.Run : PlayerState.Walk);
            return;
        }
    }

    protected void ApplyGravity()
    {
        m_Player.character.Move(new Vector3(0f, m_Player.config.gravity * Time.deltaTime, 0f));
    }
}
