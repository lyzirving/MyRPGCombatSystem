using UnityEngine;
using AnimationDefine;
using UnityEngine.Events;

public class PlayerModel : MonoBehaviour
{
    private Animator m_Animator;
    private UnityAction m_LeftFootStepAc;
    private UnityAction m_RightFootStepAc;

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

    public void StartAnimation(int hash)
    {
        m_Animator?.SetBool(hash, true);
    }

    public void StopAnimation(int hash)
    {
        m_Animator?.SetBool(hash, false);
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

    private void OnLeftFootStep()
    {
        m_LeftFootStepAc?.Invoke();
    }

    private void OnRightFootStep()
    {
        m_RightFootStepAc?.Invoke();
    }
}
