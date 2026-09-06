using System.Collections;
using UnityEngine;

public delegate void RootMotionAction(Vector3 deltaPosition, Quaternion deltaRotation);
public delegate void IKAction(int layerIndex);

public class CharacterModel : MonoBehaviour
{
    protected readonly WaitForSecondsRealtime HIT_STOP_WAIT_TIME = new WaitForSecondsRealtime(0.1f);

    protected Animator m_Animator;
    protected ICharacterBehavior m_CharacterBehaviour;
    protected Coroutine m_HitStopCoroutine;
    protected float m_HitStopOriginTimeScale = 1f;
    protected float m_HitStopOriginAnimSpeed = 1f;
    protected event RootMotionAction m_RootMotionAc;
    protected event IKAction m_IKAc;

    public Animator animator => m_Animator;
    public bool isHitStopRunning => m_HitStopCoroutine != null;

    #region State Methods
    private void Awake()
    {
        m_Animator = GetComponentInChildren<Animator>();
    }

    private void OnAnimatorMove()
    {
        m_RootMotionAc?.Invoke(m_Animator.deltaPosition, m_Animator.deltaRotation);
    }

    /// <summary>
    /// Animator IK Callback（should select IK Pass in animator layer）。
    /// </summary>
    private void OnAnimatorIK(int layerIndex)
    {
        m_IKAc?.Invoke(layerIndex);
    }

    private void OnDestroy()
    {
        m_CharacterBehaviour = null;
    }

    public virtual void Init(ICharacterBehavior characterBehavior)
    {
        m_CharacterBehaviour = characterBehavior;
    }
    #endregion

    #region Animation Methods  
    /// <summary>
    /// Trigger hit stop effect on this character.
    /// </summary>
    /// <param name="slowMotionScale">Animator speed during freeze. 0 = total freeze.</param>
    /// <param name="duration">Freeze duration in real-time seconds.</param>     
    public void HitStop(float timeScale = 0.1f,  float animatorSpeed = 0.18f, float duration = 0.06f)
    {
        if (m_HitStopCoroutine != null)
        {
            MonoManager.Stop(m_HitStopCoroutine);           
            m_Animator.speed = m_HitStopOriginAnimSpeed;
            Time.timeScale = m_HitStopOriginTimeScale;
            m_HitStopCoroutine = null;
        }
        m_HitStopCoroutine = MonoManager.Run(HitStopCoroutine(timeScale, animatorSpeed, duration));
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

    private IEnumerator HitStopCoroutine(float timeScale, float animSpeed, float duration)
    {        
        m_HitStopOriginAnimSpeed = m_Animator.speed;
        m_HitStopOriginTimeScale = Time.timeScale;

        m_Animator.speed = animSpeed;
        Time.timeScale = timeScale;    

        yield return new WaitForSeconds(duration);

        m_Animator.speed = m_HitStopOriginAnimSpeed;
        Time.timeScale = m_HitStopOriginTimeScale;    
            
        m_HitStopCoroutine = null;
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

    public void RegisterIKAction(IKAction action)
    {
        m_IKAc += action;
    }

    public void RemoveIKAction(IKAction action)
    {
        m_IKAc -= action;
    }

    public void ClearAllAction()
    {
        m_RootMotionAc = null;
    }
    #endregion
}
