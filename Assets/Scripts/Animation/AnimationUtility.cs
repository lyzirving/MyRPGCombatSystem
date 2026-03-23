using UnityEngine;

public static class AnimationUtility
{
    /// Extensions
    public static bool IsTransitToState(this Animator animator, string nextStateName, int layerIndex)
    {
        return animator.IsInTransition(layerIndex) && animator.GetNextAnimatorStateInfo(layerIndex).IsName(nextStateName);
    }

    public static bool GetNormalizedTime(this Animator animator, string animatorStateName, int layerIndex, out float time)
    {
        if (animator.IsInTransition(layerIndex))
        {
            var nextState = animator.GetNextAnimatorStateInfo(layerIndex);
            if (nextState.IsName(animatorStateName))
            {
                time = nextState.normalizedTime % 1f;
                return true;
            }
        }

        var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
        if (stateInfo.IsName(animatorStateName))
        {
            time = stateInfo.normalizedTime % 1f;
            return true;
        }
        time = 0f;
        return false;
    }
}
