using UnityEngine;
using AnimationDefine;
using UnityEngine.Events;
using System;

public delegate void RootMotionAction(Vector3 deltaPosition, Quaternion deltaRotation);

public class PlayerModel : MonoBehaviour
{
    private Animator m_Animator;
    private UnityAction m_LeftFootStepAc;
    private UnityAction m_RightFootStepAc;

    private event RootMotionAction m_RootMotionAc;

    #region State Methods
    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        if (m_Animator == null)
            throw new System.Exception("Err, Animator hasn't been assigned");
    }

    private void Start()
    {
        AnimationEventReceiver.instance.RegisterHandler(PlayerAnimationEvent.LeftFootStep, OnLeftFootStep);
        AnimationEventReceiver.instance.RegisterHandler(PlayerAnimationEvent.RightFootStep, OnRightFootStep);
    }

    private void OnDisable()
    {
        AnimationEventReceiver.instance?.RemoveHandler(PlayerAnimationEvent.LeftFootStep, OnLeftFootStep);
        AnimationEventReceiver.instance?.RemoveHandler(PlayerAnimationEvent.RightFootStep, OnRightFootStep);
    }

    private void OnAnimatorMove()
    {
        m_RootMotionAc?.Invoke(m_Animator.deltaPosition, m_Animator.deltaRotation);
    }
    #endregion

    #region Main Methods
    public void StartAnimation(int hash)
    {
        m_Animator?.SetBool(hash, true);
    }

    public void StopAnimation(int hash)
    {
        m_Animator?.SetBool(hash, false);
    }

    public void RegisterRootMotionAction(RootMotionAction action)
    {
        m_RootMotionAc += action;
    }

    public void RemoveRootMotionAction(RootMotionAction action)
    {
        m_RootMotionAc -= action;
    }

    public void RegisterLeftFootStepAction(UnityAction action)
    {
        m_LeftFootStepAc += action;
    }

    public void RemoveLeftFootStepAction(UnityAction action)
    {
        m_LeftFootStepAc -= action;
    }

    public void RegisterRightFootStepAction(UnityAction action)
    {
        m_RightFootStepAc += action;
    }

    public void RemoveRightFootStepAction(UnityAction action)
    {
        m_RightFootStepAc -= action;
    }

    public void ClearAllAction()
    {
        m_RootMotionAc = null;
        m_LeftFootStepAc = null;
        m_RightFootStepAc = null;
    }
    #endregion

    #region Behavior Methods
    private void OnLeftFootStep()
    {
        m_LeftFootStepAc?.Invoke();
    }

    private void OnRightFootStep()
    {
        m_RightFootStepAc?.Invoke();
    }
    #endregion
}
