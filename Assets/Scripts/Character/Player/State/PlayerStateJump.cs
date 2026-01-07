using UnityEngine;

public class PlayerStateJump : PlayerStateMove
{
    public override void Enter(StateBase exitState)
    {
        base.Enter(exitState);
        m_Player.model.StartAnimation(m_Player.animConsts.jumpHash);
        m_Player.model.RegisterRootMotionAction(HandleRootMotion);
    }

    public override void Exit(StateBase newState)
    {
        m_Player.model.RemoveRootMotionAction(HandleRootMotion);
        m_Player.model.StopAnimation(m_Player.animConsts.jumpHash);
        base.Exit(newState);
    }

    public override void Update()
    {        
    }

    private void HandleRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        m_Player.transform.position += deltaPosition;
    }
}
