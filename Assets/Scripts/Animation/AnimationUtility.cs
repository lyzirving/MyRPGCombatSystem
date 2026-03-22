using UnityEngine;

public static class AnimationUtility
{
    /// Extensions
    public static bool IsTransitToState(this Animator animator, string nextStateName, int layerIndex)
    {
        return animator.IsInTransition(layerIndex) && animator.GetNextAnimatorStateInfo(layerIndex).IsName(nextStateName);
    }
}
