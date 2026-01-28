using UnityEngine;
using UnityEngine.Events;

public delegate void RootMotionAction(Vector3 deltaPosition, Quaternion deltaRotation);

public class PlayerModel : MonoBehaviour
{
    private Animator m_Animator;
    private WeaponController[] m_Weapons;
    private IPlayerBehavior m_PlayerBehaviour;

    private UnityAction m_LeftFootStepAc;
    private UnityAction m_RightFootStepAc;

    private event RootMotionAction m_RootMotionAc;

    private int m_ArmedWeapon = -1;

    public WeaponController[] weapons { get { return m_Weapons; }  }
    public int armedWeaponIndex
    {
        get { return m_ArmedWeapon; }
        set { m_ArmedWeapon = value; }
    }

    #region State Methods
    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        if (m_Animator == null)
            throw new System.Exception("Err, Animator hasn't been assigned");        
    }

    private void Start()
    {       
        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.LeftFootStep, OnLeftFootStep);
        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.RightFootStep, OnRightFootStep);

        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.AttackStart, OnAttackStart);
        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.AttackEnd, OnAttackEnd);
    }

    private void OnDisable()
    {
        AnimationEventReceiver.instance?.RemoveAction(AnimationEventType.LeftFootStep, OnLeftFootStep);
        AnimationEventReceiver.instance?.RemoveAction(AnimationEventType.RightFootStep, OnRightFootStep);

        AnimationEventReceiver.instance?.RemoveAction(AnimationEventType.AttackStart, OnAttackStart);
        AnimationEventReceiver.instance?.RemoveAction(AnimationEventType.AttackEnd, OnAttackEnd);
    }

    private void OnAnimatorMove()
    {
        m_RootMotionAc?.Invoke(m_Animator.deltaPosition, m_Animator.deltaRotation);
    }

    private void OnDestroy()
    {
        m_PlayerBehaviour = null;
    }
    #endregion

    #region Main Methods
    public void Init(IPlayerBehavior playerBehavior)
    {
        m_PlayerBehaviour = playerBehavior;
        m_Weapons = GetComponentsInChildren<WeaponController>();
        int len = m_Weapons == null ? 0 : m_Weapons.Length;
        Debug.Log($"Find weapons[{len}] in nodes");
        if (m_Weapons != null)
        {
            for (int i = 0; i < m_Weapons.Length; i++)
                m_Weapons[i].Init(playerBehavior);
        }

        if (len != 0)
            m_ArmedWeapon = 0;
    }

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

    private void OnLeftFootStep(in AnimationEventInfo info)
    {
        m_LeftFootStepAc?.Invoke();
    }

    private void OnRightFootStep(in AnimationEventInfo info)
    {
        m_RightFootStepAc?.Invoke();
    }

    private void OnAttackStart(in AnimationEventInfo info)
    {
        m_PlayerBehaviour?.OnStartAttack(m_Weapons[m_ArmedWeapon].skillConfig);
        m_Weapons?[m_ArmedWeapon].OnStartAttack();
    }

    private void OnAttackEnd(in AnimationEventInfo info)
    {
        m_PlayerBehaviour?.OnStopAttack(m_Weapons[m_ArmedWeapon].skillConfig);
        m_Weapons?[m_ArmedWeapon].OnStopAttack();
    }
    #endregion
}
