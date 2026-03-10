
/// <summary>
/// Final Animation = Base Layer + Additive Layer(Current Pose - Reference Pose)
/// Reference Pose is by default the first frame of the clip.
/// </summary>
public class AIStateHurt : AIStateGround
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_AIController.model.StartAnimation(AnimationConsts.hit);
    }

    public override void ReEnter(ChangeStateArgs args)
    {
        m_AIController.model.StartAnimation(AnimationConsts.hurt, 0.01f, AnimationConsts.HURT_LAYER);
    }

    public override void Exit(StateBase newState)
    {
        m_AIController.model.StopAnimation(AnimationConsts.hit);
    }

    public override void Update()
    {
        var state = m_AIController.model.animator.GetCurrentAnimatorStateInfo(AnimationConsts.HURT_LAYER);
        float time = state.normalizedTime % 1f;
        if (time >= 0.9f)
        {
            m_AIController.ChangeState(ECharacterState.Idle);
        }
    }
}
