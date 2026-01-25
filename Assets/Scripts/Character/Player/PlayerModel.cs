using UnityEngine;
using UnityEngine.Events;

public delegate void RootMotionAction(Vector3 deltaPosition, Quaternion deltaRotation);

public class PlayerModel : MonoBehaviour
{
    private Animator m_Animator;
    private WeaponController[] m_Weapons;

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
        m_Weapons = GetComponentsInChildren<WeaponController>();
        int len = m_Weapons == null ? 0 : m_Weapons.Length;
        Debug.Log($"Find weapons[{len}] in nodes");
        if (m_Weapons != null)
        {
            for (int i = 0; i < m_Weapons.Length; i++)
            {
                m_Weapons[i].Init();
            }
        }

        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.LeftFootStep, OnLeftFootStep);
        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.RightFootStep, OnRightFootStep);
    }

    private void OnDisable()
    {
        AnimationEventReceiver.instance?.RemoveAction(AnimationEventType.LeftFootStep, OnLeftFootStep);
        AnimationEventReceiver.instance?.RemoveAction(AnimationEventType.RightFootStep, OnRightFootStep);
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
    private void OnLeftFootStep(AnimationEventInfo info)
    {
        m_LeftFootStepAc?.Invoke();
    }

    private void OnRightFootStep(AnimationEventInfo info)
    {
        m_RightFootStepAc?.Invoke();
    }
    #endregion
}
