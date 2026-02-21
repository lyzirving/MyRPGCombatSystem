
public class PlayerStateWalk : PlayerStateMove
{    
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);        
        m_Player.model.StartAnimation(AnimationConsts.walk);

        m_Player.attrs.speedModify = m_Player.config.walkSpeedModify;
        m_Player.attrs.jumpForce = m_Player.config.weakJumpForce;
    }

    public override void Exit(StateBase newState)
    {        
        m_Player.model.StopAnimation(AnimationConsts.walk);
        base.Exit(newState);
    }

    public override void Update()
    {
        if (m_Player.action.shouldRun)
        {
            m_Player.ChangeState(ECharacterState.Run);
            return;
        }

        base.Update();
    }    
}
