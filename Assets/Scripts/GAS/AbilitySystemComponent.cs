using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySystemComponent : MonoBehaviour
{
    public List<GameplayAttribute> grantedAttributes = new List<GameplayAttribute>();    
    public List<GameplayAbility> grantedAbilities = new List<GameplayAbility>();
    public List<GameplayEffect> permanentEffects = new List<GameplayEffect>();
    public GameplayTagContainer activeTags = new GameplayTagContainer();

    public event System.Action<GameplayAttribute> onAttributeChanged;
    public event System.Action<GameplayTag> onTagAdded;
    public event System.Action<GameplayTag> onTagRemoved;
    public event System.Action<GameplayEffect> onEffectApplied;
    public event System.Action<GameplayEffect> onEffectRemoved;
    
    private List<ActiveGameplayEffect> m_ActiveEffects = new List<ActiveGameplayEffect>();
    private Dictionary<int, GameplayAbility> m_ActiveAbilities = new Dictionary<int, GameplayAbility>();

    private GameplayAttributeSet m_AttributeSet = new GameplayAttributeSet();

    private void Awake()
    {
        InitializeAttributes();
        ApplyPermanentEffects();
        GrantAbilities();
    }

    private void Update()
    {
        foreach (var item in m_ActiveAbilities)
        {
            item.Value.OnUpdate(Time.deltaTime);
        }
    }

    #region Main Methods
    public void Reset()
    {
        foreach (var effect in m_ActiveEffects)
        {
            RemoveActiveEffect(effect);
        }

        m_AttributeSet.ClearAllModifiers();

        activeTags.Clear();

        InitializeAttributes();
        ApplyPermanentEffects();
    }

    private void InitializeAttributes()
    {
        m_AttributeSet.Clear();
        foreach (var attr in grantedAttributes)
            m_AttributeSet.Add(attr);
    }

    private void ApplyPermanentEffects()
    {
        foreach (var effect in permanentEffects)
        {
            ApplyEffect(effect, effect.isInstant, this);
        }
    }

    private void GrantAbilities()
    {
        foreach (var ability in grantedAbilities)
        {
            if (ability.activationPolicy == AbilityActivationPolicy.OnSpawn)
            {
                TryActivateAbility(ability);
            }
        }
    }
    #endregion    

    #region Ability Operations
    public bool TryActivateAbility(GameplayAbility ability, object target = null)
    {
        if (ability == null)
            return false;

        if (ability.isActive)
            return true;

        if (ability.Activate(this, target))
        {
            if(!ability.isInstant)
                m_ActiveAbilities[ability.id] = ability;
            return true;
        }
        
        return false;
    }

    public bool CancelAbility(GameplayAbility ability)
    {
        if(ability == null || !ability.canBeCanceled)
            return false;

        ability.EndAbility(true);

        return m_ActiveAbilities.Remove(ability.id);
    }

    public void CancelAllAbilities()
    { 
        List<GameplayAbility> toBeRemoved = new List<GameplayAbility>();
        foreach (var item in m_ActiveAbilities)
        {
            var ability = item.Value;
            if (ability.canBeCanceled)
            {
                toBeRemoved.Add(ability);
                ability.EndAbility(true);
            }
        }

        foreach (var item in toBeRemoved)
        {
            m_ActiveAbilities.Remove(item.id);
        }
    }

    public void OnAbilityEnded(GameplayAbility ability)
    { 
    }

    public void OnAbilityCanceled(GameplayAbility ability)
    {
    }
    #endregion

    #region Effect Operations
    public bool ApplyEffect(GameplayEffect effect, object source = null)
    {
        return ApplyEffect(effect, effect.isInstant, source);
    }

    public void ApplyInstantEffect(GameplayEffect effect, object source = null)
    {
        ApplyEffect(effect, true, source);
    }

    public void ApplyContinuousEffect(GameplayEffect effect, object source = null)
    {
        ApplyEffect(effect, false, source);
    }

    private bool ApplyEffect(GameplayEffect effect, bool isInstant, object source)
    {
        if (effect == null || !effect.CanApplyTo(this))
            return false;

        if (isInstant)
        {
            m_AttributeSet.ApplyEffectImmediately(effect, onAttributeChanged);
        }
        else
        {
            foreach (var tag in effect.grantedTags)
                AddTag(tag);

            foreach (var tag in effect.removedTags)
                RemoveTag(tag);

            m_AttributeSet.ApplyEffect(effect, onAttributeChanged);

            var activeEffect = new ActiveGameplayEffect(effect, Time.time, source, this);
            m_ActiveEffects.Add(activeEffect);

            if (effect.durationType == EffectDurationType.Duration)
            {
                StartCoroutine(RemoveEffectAfterDuration(activeEffect, effect.duration));
            }
            else if (effect.durationType == EffectDurationType.Periodical)
            {
                activeEffect.StartPeriodic(this);
            }
        }
        onEffectApplied?.Invoke(effect);
        return true;
    }    

    public bool RemoveEffect(GameplayEffect effect)
    {
        List<ActiveGameplayEffect> effectsToRemove = new List<ActiveGameplayEffect>();
        foreach (var activeEffect in m_ActiveEffects)
        {
            if(activeEffect.effect == effect)
                effectsToRemove.Add(activeEffect);
        }

        foreach (var activeEffect in effectsToRemove)
        {
            RemoveActiveEffect(activeEffect);
        }

        return effectsToRemove.Count > 0;
    }

    public bool RemoveAllEffectsFromSource(object source)
    {
        List<ActiveGameplayEffect> effectsToRemove = new List<ActiveGameplayEffect>();
        foreach (var activeEffect in m_ActiveEffects)
        {
            if (activeEffect.source == source)
                effectsToRemove.Add(activeEffect);
        }

        foreach (var activeEffect in effectsToRemove)
        {
            RemoveActiveEffect(activeEffect);
        }

        return effectsToRemove.Count > 0;
    }

    private void RemoveActiveEffect(ActiveGameplayEffect activeEffect)
    {
        if (activeEffect == null)
            return;

        activeEffect.StopPeriodic(this);

        foreach (var tag in activeEffect.effect.grantedTags)
            RemoveTag(tag);

        m_AttributeSet.RemoveEffectModifiers(activeEffect.effect);        

        if(m_ActiveEffects.Remove(activeEffect))
            onEffectRemoved?.Invoke(activeEffect.effect);
    }    

    private IEnumerator RemoveEffectAfterDuration(ActiveGameplayEffect effect, float duration)
    {
        while (Time.time - effect.startTime < duration)
            yield return null;

        if (m_ActiveEffects.Contains(effect))
            RemoveActiveEffect(effect);
    }
    #endregion

    #region Tag Operations
    public bool HasTag(GameplayTag tag)
    {
        return activeTags.HasTag(tag);
    }

    public bool HasAllTags(IEnumerable<GameplayTag> tags)
    {
        return activeTags.HasAllTags(tags);
    }

    public bool HasAnyTag(IEnumerable<GameplayTag> tags)
    {
        return activeTags.HasAnyTag(tags);
    }

    public void AddTag(GameplayTag tag)
    {
        if (!HasTag(tag))
        {
            activeTags.AddTag(tag);
            onTagAdded?.Invoke(tag);
        }
    }

    public bool RemoveTag(GameplayTag tag)
    {
        bool removed = activeTags.RemoveTag(tag);
        if (removed)
        {
            onTagRemoved?.Invoke(tag);
        }
        return removed;
    }
    #endregion

    #region Cost Operations
    public bool CanPay(AbilityCost cost)
    { 
        return m_AttributeSet.CanPay(cost);
    }

    public void Pay(AbilityCost cost)
    {
        m_AttributeSet.Pay(cost, onAttributeChanged);
    }
    #endregion
}
