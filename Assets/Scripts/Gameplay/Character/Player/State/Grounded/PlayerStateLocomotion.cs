using UnityEngine;

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

    protected void UpdateFootStep(int currentLoop, float currtentTime, int lastLoop, float lastTime)
    {
        if (currentLoop != lastLoop && Mathf.Abs(currentLoop - lastLoop) == 1)
        {
            m_Player.OnFootStep(EFootstep.RightFootstep);
        }

        if (lastTime < 0.5f && currtentTime >= 0.5f && !Mathf.Approximately(lastTime, 0f))
        {
            m_Player.OnFootStep(EFootstep.LeftFootstep);
        }
    }
}
