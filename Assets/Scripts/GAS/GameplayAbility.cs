using System.Collections.Generic;
using UnityEngine;

public enum AbilityActivationPolicy
{
    OnSpawn,
    OnInput,
    Passive 
}

public enum AbilityCooldownType
{
    None,    
    ByTime,  
    ByCharges
}

public class AbilityCost
{
    public EGameplayAttributeType attribute;
    public float value;
}

[CreateAssetMenu(fileName = "NewGameplayAbility", menuName = "GAS/GameplayAbility")]
public class GameplayAbility : ScriptableObject
{
    protected static int k_GlobalId = 0;

    [Header("Basic")]
    public string abilityName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Activation Strategy")]
    public AbilityActivationPolicy activationPolicy = AbilityActivationPolicy.OnInput;
    public float duration = 0f;
    public bool canBeCanceled = true;

    [Header("Cooldown Settings")]
    public AbilityCooldownType cooldownType = AbilityCooldownType.ByTime;
    public float cooldownTime = 1f;
    public int maxCharges = 1;

    [Header("Tags")]
    public List<GameplayTag> requiredTags = new List<GameplayTag>();
    public List<GameplayTag> blockedTags = new List<GameplayTag>();

    [Header("Cost Settings")]
    public List<AbilityCost> costs = new List<AbilityCost>();

    [Header("Effect List")]
    public List<GameplayEffect> effects = new List<GameplayEffect>();

    public int id => m_Id;
    public bool isInstant => Mathf.Abs(m_EndTime - m_ActiveTime) < Mathf.Epsilon;
    public bool isPlaying => Time.time > (m_ActiveTime + duration);
    public bool isActive => m_IsActive;

    private int m_Id;    
    private AbilitySystemComponent m_ASC;
    private bool m_IsActive = false;
    private float m_ActiveTime;
    private float m_EndTime;
    private object m_Target;

    private void Awake()
    {
        m_Id = k_GlobalId++;
    }

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
        m_EndTime = m_ActiveTime + duration;

        PayCost();
        StartCooldown();

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

        Reset();
    }

    public virtual bool CanActivate(AbilitySystemComponent owner)
    {
        if (owner.HasAnyTag(blockedTags))
            return false;

        if (requiredTags.Count > 0 && !owner.HasAllTags(requiredTags))
            return false;

        if (IsOnCooldown())
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
    }

    protected virtual bool IsOnCooldown()
    {
        return false;
    }

    protected virtual void Reset()
    {
        m_Target = null;
        m_ASC = null;
        m_IsActive = false;

        m_ActiveTime = m_EndTime = 0f;
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
