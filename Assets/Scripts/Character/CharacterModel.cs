using UnityEngine;
using UnityEngine.Events;

public delegate void RootMotionAction(Vector3 deltaPosition, Quaternion deltaRotation);

public class CharacterModel : MonoBehaviour
{
    protected Animator m_Animator;
    protected ICharacterBehavior m_CharacterBehaviour;

    protected event RootMotionAction m_RootMotionAc;

    public Animator animator => m_Animator;

    #region State Methods
    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    private void Start()
    {
        AnimationEventReceiver.instance.RegisterAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackStart, OnAttackStart);
        AnimationEventReceiver.instance.RegisterAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackEnd, OnAttackEnd);
    }

    private void OnDisable()
    {
        AnimationEventReceiver.instance?.RemoveAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackStart, OnAttackStart);
        AnimationEventReceiver.instance?.RemoveAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackEnd, OnAttackEnd);
    }

    private void OnAnimatorMove()
    {
        m_RootMotionAc?.Invoke(m_Animator.deltaPosition, m_Animator.deltaRotation);
    }

    private void OnDestroy()
    {
        m_CharacterBehaviour = null;
    }

    public void Init(ICharacterBehavior characterBehavior)
    {
        m_CharacterBehaviour = characterBehavior;
    }
    #endregion

    #region Animation Methods    
    public void StartAnimation(int hash)
    {
        m_Animator?.SetBool(hash, true);
    }

    public void StartAnimation(string name, float fixedTransitionDuration = 0.25f)
    {
        m_Animator?.CrossFadeInFixedTime(name, fixedTransitionDuration);
    }

    public void SetAnimationFloat(int nameHash, float value)
    {
        m_Animator?.SetFloat(nameHash, value);
    }

    public void SetAnimationFloat(int nameHash, float value, float dampTime, float deltaTime)
    {
        m_Animator?.SetFloat(nameHash, value, dampTime, deltaTime);
    }

    public float GetAnimationFloat(int nameHash)
    {
        return m_Animator.GetFloat(nameHash);
    }

    public void SetLayerWeight(int layer, float weight)
    {
        m_Animator?.SetLayerWeight(layer, weight);
    }

    public int GetLayerIndex(string layerName)
    {
        return m_Animator?.GetLayerIndex(layerName) ?? -1;
    }    

    public void StopAnimation(int hash)
    {
        m_Animator?.SetBool(hash, false);
    }
    #endregion

    #region Listener Methods
    public void RegisterRootMotionAction(RootMotionAction action)
    {
        m_RootMotionAc += action;
    }

    public void RemoveRootMotionAction(RootMotionAction action)
    {
        m_RootMotionAc -= action;
    }  

    public void ClearAllAction()
    {
        m_RootMotionAc = null;
    }
    #endregion

    #region AnimationEvent Handler
    private void OnAttackStart(in AnimationEventInfo info)
    {
        m_CharacterBehaviour?.OnAttackBegin();
    }

    private void OnAttackEnd(in AnimationEventInfo info)
    {
        m_CharacterBehaviour?.OnAttackEnd();
    }
    #endregion AnimationEvent Handler
}
