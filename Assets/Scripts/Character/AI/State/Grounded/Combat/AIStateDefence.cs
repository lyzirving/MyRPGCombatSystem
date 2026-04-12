using UnityEngine;

public class AIStateDefence : AIStateCombat
{
    private EDefenceState m_SubState;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_SubState = EDefenceState.Enter;
        m_AIController.model.SetAnimationBool(AnimationConsts.defenceRelease, false);
        m_AIController.model.StartAnimation(AnimationConsts.defence);
    }

    public override void ReEnter(ChangeStateArgs args)
    {
        //do nothing here
    }

    public override bool Exit(StateBase newState)
    {
        if(m_SubState == EDefenceState.Exiting)
            return false;

        m_AIController.model.StopAnimation(AnimationConsts.defence);
        base.Exit(newState);
        return true;
    }

    public override void Update()
    {
        if (m_SubState == EDefenceState.Exiting)
        {
            if (m_AIController.model.animator.GetTargetAnimationTime("DefenceEnd", AnimationConsts.BASE_LAYER, out float time))
            {
                if (time > 0.7f)
                {
                    m_SubState = EDefenceState.End;
                    m_AIController.ChangeState(ECharacterState.Idle);
                    return;
                }
            }
        }
    }

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Defence;
    }

    public void ReleaseDefence()
    {
        m_AIController.model.SetAnimationBool(AnimationConsts.defenceRelease, true);
        m_SubState = EDefenceState.Exiting;
    }
}
