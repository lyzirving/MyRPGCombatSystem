using UnityEngine;

public class PlayerStateFall : PlayerStateAirborne
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        float feetTween = (args.footStep == EFootstep.None) ? 0f : (args.footStep == EFootstep.LeftFootstep) ? 2.1f : -2.1f;
        m_Player.model.SetAnimationFloat(AnimationConsts.feetTween, feetTween);
        m_Player.model.SetAnimationFloat(AnimationConsts.jumpRatio, -4f);
    }

    public override void Update()
    {
        float ratio = m_Player.model.GetAnimationFloat(AnimationConsts.jumpRatio);
        m_Player.model.SetAnimationFloat(AnimationConsts.jumpRatio, ratio - Time.deltaTime);
    }

    public override void FixedUpdate()
    {
        // The fall phase also responds to input with air control (speed factor / rotation
        // damping / acceleration damping, shared with Jump)
        UpdateAirborneMovement();
    }
}
