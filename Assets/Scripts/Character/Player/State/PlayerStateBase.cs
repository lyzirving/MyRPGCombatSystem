using UnityEngine;

public class PlayerStateBase : CharacterStateBase
{    
    protected PlayerController m_Player;

    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        m_Player = owner as PlayerController;
    }

    protected void GetCurrentAnimationTimeInfo(out int loop, out float time)
    {
        // Note: Only consider the case when entering the move animation.
        //       If character quits the animation, the Update() will not run.
        if (m_Player.model.animator.IsInTransition(AnimationConsts.BASE_LAYER))
        {
            var state = m_Player.model.animator.GetNextAnimatorStateInfo(AnimationConsts.BASE_LAYER);
            loop = Mathf.FloorToInt(state.normalizedTime);
            time = state.normalizedTime % 1f;
        }
        else
        {
            var state = m_Player.model.animator.GetCurrentAnimatorStateInfo(AnimationConsts.BASE_LAYER);
            loop = Mathf.FloorToInt(state.normalizedTime);
            time = state.normalizedTime % 1f;
        }
    }
}
