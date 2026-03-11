
using UnityEngine;

/// <summary>
/// Final Animation = Base Layer + Additive Layer(Current Pose - Reference Pose)
/// Reference Pose is by default the first frame of the clip.
/// </summary>
public class AIStateHurt : AIStateGround
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_AIController.model.StartAnimation(AnimationConsts.hit);
        m_AIController.model.SetAnimationFloat(AnimationConsts.hitTween, CalcHitTween(args.hitPos));
        m_AIController.model.SetAnimationFloat(AnimationConsts.hitRatio, Random.Range(0f, 0.5f));
    }

    public override void ReEnter(ChangeStateArgs args)
    {
        m_AIController.model.StartAnimation(AnimationConsts.hurt, 0.01f, AnimationConsts.HURT_LAYER);
        m_AIController.model.SetAnimationFloat(AnimationConsts.hitTween, CalcHitTween(args.hitPos));
        m_AIController.model.SetAnimationFloat(AnimationConsts.hitRatio, Random.Range(0f, 0.5f));
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

    private float CalcHitTween(Vector3 hitPos)
    {
        Vector3 l = hitPos - m_AIController.capsule.bounds.center;
        bool isRight = Vector3.Cross(m_AIController.transform.forward, l.normalized).y > 0f;
        float ratio = Mathf.Abs(Vector3.Dot(l.normalized, m_AIController.transform.right)) / m_AIController.capsule.bounds.extents.x;
        return ratio * (isRight ? -1f : 1f);
    }
}
