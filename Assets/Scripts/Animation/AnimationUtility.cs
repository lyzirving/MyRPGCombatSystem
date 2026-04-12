using UnityEngine;

public static class AnimationUtility
{
    /// Extensions
    public static bool IsTransitToState(this Animator animator, string nextStateName, int layerIndex)
    {
        return animator.IsInTransition(layerIndex) && animator.GetNextAnimatorStateInfo(layerIndex).IsName(nextStateName);
    }  

    public static bool IsTargetAnimation(this Animator animator, string target, int layer)
    {
        if (animator.IsInTransition(layer))
        {
            return animator.GetNextAnimatorStateInfo(layer).IsName(target);
        }
        return animator.GetCurrentAnimatorStateInfo(layer).IsName(target);
    }

    public static bool GetTargetAnimationTime(this Animator animator, string target, int layer, out float time)
    {
        if (animator.IsInTransition(layer))
        {
            var nextInfo = animator.GetNextAnimatorStateInfo(layer);
            if (nextInfo.IsName(target))
            {
                time = nextInfo.normalizedTime % 1f;
                return true;
            }
        }

        var info = animator.GetCurrentAnimatorStateInfo(layer);
        if (info.IsName(target))
        {
            time = info.normalizedTime % 1f;
            return true;
        }

        time = 0f;
        return false;
    }
}
