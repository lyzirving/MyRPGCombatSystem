
public class PlayerStateWalk : PlayerStateMove
{    
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);        
        m_Player.model.StartAnimation(m_Player.animConsts.walkHash);

        m_Player.attrs.speedModify = m_Player.config.walkSpeedModify;
        m_Player.attrs.jumpForce = m_Player.config.weakJumpForce;
    }

    public override void Exit(StateBase newState)
    {        
        m_Player.model.StopAnimation(m_Player.animConsts.walkHash);
        base.Exit(newState);
    }

    public override void Update()
    {
        if (InputManager.instance.shouldPlayerRun)
        {
            m_Player.ChangeState(EPlayerState.Run);
            return;
        }

        base.Update();
    }    
}
