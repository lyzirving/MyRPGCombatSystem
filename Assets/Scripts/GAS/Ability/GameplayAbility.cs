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

[Serializable]
public class AbilityCost
{
    public EGameplayAttributeType attribute;
    public float value;
}

/// <summary>
/// Gameplay ability class
/// Instant Gameplay ability should end itself mannually
/// </summary>
public abstract class GameplayAbility : ScriptableObject
{
    [HideInInspector][SerializeField] private string m_UniqueID;

#if UNITY_EDITOR
    public void SetUniqueID(string id) => m_UniqueID = id;
#endif

    public string guid => m_UniqueID;

    [Header("Basic")]
    public string abilityName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Activation Strategy")]
    public AbilityActivationPolicy activationPolicy = AbilityActivationPolicy.OnInput;
    public float abilityDuration = 0f;
    public bool canBeCanceled = true;

    [Header("Cost Settings")]
    public List<AbilityCost> costs = new List<AbilityCost>();

    [Header("Effect List")]
    public List<GameplayEffect> effects = new List<GameplayEffect>();

    public event Action<float> onCooldownProgressChange;
    public event Action onCooldownStart;
    public event Action onCooldownEnd;

    [HideInInspector] public GameplayTag cooldownTag;
    [HideInInspector] public float cooldownDuration = 0f;
    [HideInInspector] public GameplayEffect cooldownEffect;

    [HideInInspector] public List<GameplayTag> grantedTags = new List<GameplayTag>();
    [HideInInspector] public List<GameplayTag> requiredTags = new List<GameplayTag>();
    [HideInInspector] public List<GameplayTag> blockedTags = new List<GameplayTag>();

    private Coroutine m_CooldownHandle;
    private float m_CooldownDuration;
    private float m_CooldownStartTime;

    public int classHash
    {
        get
        {
            if (m_ClassHash == null)
            {
                var type = GetType();
                var hashField = typeof(AbilityHash<>).MakeGenericType(type)
                    .GetField(nameof(AbilityHash<GameplayAbility>.classHash),
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                m_ClassHash = (int)hashField.GetValue(null);
            }
            return m_ClassHash.Value;
        }
    }
    public bool isInstant => Mathf.Abs(m_EndTime - m_ActiveTime) < Mathf.Epsilon;
    public bool isActive => m_IsActive;

    [NonSerialized] private AbilitySystemComponent m_ASC;
    [NonSerialized] private bool m_IsActive = false;
    [NonSerialized] private float m_ActiveTime;
    [NonSerialized] private float m_EndTime;
    [NonSerialized] private object m_Target;
    [NonSerialized] protected CharacterControllerBase m_Character;
    [NonSerialized] private int? m_ClassHash;

    public void Attach(CharacterControllerBase character)
    {
        m_Character = character;
    }

    /// <summary>
    /// Called every frame when the ability is a continuous ability
    /// </summary>
    /// <param name="deltaTime"></param>
    public virtual void OnUpdate(float deltaTime)
    {
        if (!m_IsActive)
            return;

        OnAbilityUpdate(deltaTime);

        if (!isInstant && Time.time >= m_EndTime)
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

        owner.RegisterActiveAbility(this);

        m_ASC = owner;
        m_Target = target;
        m_IsActive = true;
        m_ActiveTime = Time.time;
        m_EndTime = m_ActiveTime + abilityDuration;

        for (int i = 0; i < grantedTags.Count; ++i)
            owner.AddTag(grantedTags[i]);

        PayCost();

        foreach (var effect in effects)
            m_ASC.ApplyEffect(effect, this);

        OnAbilityActivated();

        OnAbilityPerformed();

        return true;
    }

    public virtual void ReActivate(AbilitySystemComponent owner, object target = null)
    {
        OnAbilityReEnter();
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

        m_ASC?.RemoveAllGrantedTags(this);
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
        if (cooldownTag == null || !cooldownTag.isValid || cooldownEffect == null)
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

        if (m_CooldownHandle != null)
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
    protected abstract void OnAbilityActivated();

    protected abstract void OnAbilityPerformed();

    protected abstract void OnAbilityEnded();

    protected abstract void OnAbilityCanceled();

    protected abstract void OnAbilityUpdate(float deltaTime);

    protected abstract void OnAbilityReEnter();
    #endregion
}
