using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityActivationPolicy
{
    OnSpawn,
    OnInput,
    Passive 
}

public class AbilityCost
{
    public EGameplayAttributeType attribute;
    public float value;
}

[CreateAssetMenu(fileName = "NewGameplayAbility", menuName = "GAS/GameplayAbility")]
public class GameplayAbility : ScriptableObject
{
    [Header("Basic")]
    public string abilityName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Activation Strategy")]
    public AbilityActivationPolicy activationPolicy = AbilityActivationPolicy.OnInput;
    public float abilityDuration = 0f;
    public bool canBeCanceled = true;

    [Header("Cooldown Settings")]
    public GameplayTag cooldownTag;
    public float cooldownDuration = 0f;
    public GameplayEffect cooldownEffect;
    public event Action<float> onCooldownProgressChange;
    public event Action onCooldownStart;
    public event Action onCooldownEnd;

    [Header("Tags")]
    public List<GameplayTag> requiredTags = new List<GameplayTag>();
    public List<GameplayTag> blockedTags = new List<GameplayTag>();

    [Header("Cost Settings")]
    public List<AbilityCost> costs = new List<AbilityCost>();

    [Header("Effect List")]
    public List<GameplayEffect> effects = new List<GameplayEffect>();

    private Coroutine m_CooldownHandle;
    private float m_CooldownDuration;
    private float m_CooldownStartTime;

    public bool isInstant => Mathf.Abs(m_EndTime - m_ActiveTime) < Mathf.Epsilon;
    public bool isActive => m_IsActive;

    private AbilitySystemComponent m_ASC;
    private bool m_IsActive = false;
    private float m_ActiveTime;
    private float m_EndTime;
    private object m_Target;

    /// <summary>
    /// Called every frame when the ability is a continuous ability
    /// </summary>
    /// <param name="deltaTime"></param>
    public virtual void OnUpdate(float deltaTime)
    {
        if (!m_IsActive || isInstant)
            return;

        OnAbilityUpdate(deltaTime);

        if (Time.time >= m_EndTime)
        {
            EndAbility(false);
        }       
    }

    public virtual bool Activate(AbilitySystemComponent owner, object target = null)
    {
        if (!CanActivate(owner))
        {
            Reset();
            return false;
        }

        m_ASC = owner;
        m_Target = target;
        m_IsActive = true;
        m_ActiveTime = Time.time;
        m_EndTime = m_ActiveTime + abilityDuration;

        PayCost();

        foreach (var effect in effects)
            m_ASC.ApplyEffect(effect, this);

        OnAbilityActivated();

        OnAbilityPerformed();

        if (isInstant)
        {
            EndAbility(false);
        }

        return true;
    }

    public virtual void EndAbility(bool isCanceled = false)
    {
        if (!m_IsActive)
            return;

        if (isCanceled)
        {
            OnAbilityCanceled();
            m_ASC?.OnAbilityCanceled(this);            
        }
        else
        {
            OnAbilityEnded();
            m_ASC?.OnAbilityEnded(this);
        }

        m_ASC?.RemoveAllEffectsFromSource(this);

        StartCooldown();

        Reset();
    }

    public virtual bool CanActivate(AbilitySystemComponent owner)
    {
        // prevent repeated activation
        if (m_IsActive)
            return false;

        bool hasCooldownTag = cooldownTag != null && cooldownTag.isValid && owner.HasTag(cooldownTag);
        if (hasCooldownTag || owner.IsEffectActive(cooldownEffect))
            return false;

        if (owner.HasAnyTag(blockedTags))
            return false;

        if (requiredTags.Count > 0 && !owner.HasAllTags(requiredTags))
            return false;

        if (!CanPayCost())
            return false;

        return true;
    }    

    protected virtual bool CanPayCost()
    {
        foreach (var item in costs)
        {
            if (!m_ASC.CanPay(item))
                return false;
        }
        return true;
    }

    protected virtual void PayCost()
    {
        foreach (var item in costs)
            m_ASC.Pay(item);
    }

    protected virtual void StartCooldown()
    {
        if(cooldownTag == null || !cooldownTag.isValid || cooldownEffect == null)
            return;

        if (cooldownEffect != null && cooldownEffect.durationType != EffectDurationType.Duration)
        {
            Debug.LogError("invalid cooldown effect must be EffectDurationType.Duration");
            return;
        }

        float duration = Mathf.Max(cooldownDuration, cooldownEffect != null ? cooldownEffect.duration : 0f);

        if (duration < Mathf.Epsilon)
            return;

        if (cooldownEffect != null)
            m_ASC.ApplyEffect(cooldownEffect, this);        

        m_CooldownDuration = duration;
        m_CooldownStartTime = Time.time;

        if(m_CooldownHandle != null)
            m_ASC.StopCoroutine(m_CooldownHandle);

        m_CooldownHandle = m_ASC.StartCoroutine(UpdateCooldownRoutine());
    }

    protected virtual void Reset()
    {
        m_Target = null;
        m_ASC = null;
        m_IsActive = false;

        m_ActiveTime = m_EndTime = 0f;
    }

    protected IEnumerator UpdateCooldownRoutine()
    {
        onCooldownStart?.Invoke();
        float remaining = GetCooldownRemaining();
        float progress = 1f;

        while (m_ASC != null && m_ASC.HasTag(cooldownTag) && remaining > 0f)
        {
            remaining = GetCooldownRemaining();
            progress = Mathf.Clamp01(1f - (remaining / m_CooldownDuration));
            onCooldownProgressChange?.Invoke(progress);
            yield return null;
        }

        m_CooldownHandle = null;
        m_CooldownDuration = 0f;
        onCooldownEnd?.Invoke();
    }

    protected float GetCooldownRemaining()
    {
        return Mathf.Max(0, m_CooldownDuration - (Time.time - m_CooldownStartTime));
    }

    #region Callback Methods
    protected virtual void OnAbilityActivated()
    {
    }

    protected virtual void OnAbilityPerformed()
    {
    }

    protected virtual void OnAbilityEnded()
    {
    }

    protected virtual void OnAbilityCanceled()
    {
    }

    protected virtual void OnAbilityUpdate(float deltaTime)
    { 
    }
    #endregion
}
