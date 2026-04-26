
public class PlayerStateLocomotion : PlayerStateGrounded
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_Player.model.SetAnimationBool(AnimationConsts.locomotion, true);
    }

    public override bool Exit(StateBase newState)
    {
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(PlayerStateLocomotion)))
        {
            m_Player.model.SetAnimationBool(AnimationConsts.locomotion, false);
        }
        return true;
    }
}
