using System.Collections;
using UnityEngine;

public delegate void RootMotionAction(Vector3 deltaPosition, Quaternion deltaRotation);

public class CharacterModel : MonoBehaviour
{
    protected readonly WaitForSecondsRealtime HIT_STOP_WAIT_TIME = new WaitForSecondsRealtime(0.1f);

    protected Animator m_Animator;
    protected ICharacterBehavior m_CharacterBehaviour;
    protected bool m_HitStopRunning = false;
    protected Coroutine m_HitStopCoroutine;
    protected event RootMotionAction m_RootMotionAc;

    public Animator animator => m_Animator;

    #region State Methods
    private void Awake()
    {
        m_Animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        AnimationEventReceiver.instance.RegisterAction(m_CharacterBehaviour.GUID, AnimationEventType.AttackStart, OnAttackStart);
        AnimationEventReceiver.instance.RegisterAction(m_CharacterBehaviour.GUID, AnimationEventType.AttackEnd, OnAttackEnd);
    }

    private void OnDisable()
    {
        AnimationEventReceiver.instance?.RemoveAction(m_CharacterBehaviour.GUID, AnimationEventType.AttackStart, OnAttackStart);
        AnimationEventReceiver.instance?.RemoveAction(m_CharacterBehaviour.GUID, AnimationEventType.AttackEnd, OnAttackEnd);
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
    public void HitStop(float slowMotionScale = 0.9f)
    {
        if (m_HitStopRunning && m_HitStopCoroutine != null)
        {
            MonoManager.Stop(m_HitStopCoroutine);
            m_HitStopRunning = false;
            m_Animator.speed = 1f;
        }
        m_HitStopCoroutine = MonoManager.Run(HitStopCoroutine(slowMotionScale));
    }

    public void TriggerAnimation(int hash)
    {
        m_Animator?.SetTrigger(hash);
    }

    public void StartAnimation(string name, float fixedTransitionDuration = 0.25f)
    {
        m_Animator?.CrossFadeInFixedTime(name, fixedTransitionDuration);
    }

    public void StartAnimation(int hashName, float fixedTransitionDuration, int layer)
    {
        m_Animator?.CrossFadeInFixedTime(hashName, fixedTransitionDuration, layer);
    }

    public void SetAnimationBool(int nameHash, bool value)
    {
        m_Animator?.SetBool(nameHash, value);
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
        return m_Animator?.GetFloat(nameHash) ?? 0f;
    }

    public bool GetAnimationBool(int nameHash)
    {
        return m_Animator?.GetBool(nameHash) ?? false;
    }

    public void SetLayerWeight(int layer, float weight)
    {
        m_Animator?.SetLayerWeight(layer, weight);
    }

    public int GetLayerIndex(string layerName)
    {
        return m_Animator?.GetLayerIndex(layerName) ?? -1;
    }

    private IEnumerator HitStopCoroutine(float slowMotionScale)
    {
        m_HitStopRunning = true;
        float originalSpeed = m_Animator.speed;

        m_Animator.speed = slowMotionScale;

        yield return HIT_STOP_WAIT_TIME;

        m_Animator.speed = originalSpeed;
        m_HitStopRunning = false;
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
