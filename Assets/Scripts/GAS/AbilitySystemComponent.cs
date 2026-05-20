using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilitySystemComponent : MonoBehaviour
{
    public List<GameplayAttribute> grantedAttributes = new List<GameplayAttribute>();    
    public List<GameplayAbility> grantedAbilities = new List<GameplayAbility>();
    public GameplayTagContainer activeTags = new GameplayTagContainer();

    public event System.Action<GameplayAttribute> onAttributeChanged;
    public event System.Action<GameplayTag> onTagAdded;
    public event System.Action<GameplayTag> onTagRemoved;
    public event System.Action<GameplayEffect> onEffectApplied;
    public event System.Action<GameplayEffect> onEffectRemoved;
    
    private Dictionary<int, ActiveGameplayEffect> m_ActiveEffects = new Dictionary<int, ActiveGameplayEffect>();
    private Dictionary<int, GameplayAbility> m_ActiveAbilities = new Dictionary<int, GameplayAbility>();

    private GameplayAttributeSet m_AttributeSet = new GameplayAttributeSet();

    private void Awake()
    {
        InitializeAttributes();
        GrantAbilitiesOnSpawn();
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
        RemoveAllActiveEffects();

        m_AttributeSet.ClearAllModifiers();

        activeTags.Clear();

        InitializeAttributes();
    }

    private void InitializeAttributes()
    {
        m_AttributeSet.Clear();
        foreach (var attr in grantedAttributes)
            m_AttributeSet.Add(attr);
    }

    private void GrantAbilitiesOnSpawn()
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
                m_ActiveAbilities[ability.GetInstanceID()] = ability;
            return true;
        }
        
        return false;
    }

    public bool CancelAbility(GameplayAbility ability)
    {
        if(ability == null || !ability.canBeCanceled)
            return false;

        ability.EndAbility(true);

        return m_ActiveAbilities.Remove(ability.GetInstanceID());
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
            m_ActiveAbilities.Remove(item.GetInstanceID());
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
    public bool IsEffectActive(GameplayEffect effect)
    {
        if (effect == null) 
            return false;

        return m_ActiveEffects.ContainsKey(effect.GetInstanceID());
    }

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
            m_ActiveEffects.Add(effect.GetInstanceID(), activeEffect);

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

    public bool RemoveAllEffectsFromSource(object source)
    {
        List<ActiveGameplayEffect> effectsToRemove = new List<ActiveGameplayEffect>();
        foreach (var activeEffect in m_ActiveEffects)
        {
            if (activeEffect.Value.source == source)
                effectsToRemove.Add(activeEffect.Value);
        }

        foreach (var itemToBeRemove in effectsToRemove)
        {
            RemoveActiveEffect(itemToBeRemove);
        }

        return effectsToRemove.Count > 0;
    }

    private void RemoveAllActiveEffects()
    {
        var affectList = m_ActiveEffects.ToList();
        foreach (var item in affectList)
            RemoveActiveEffect(item.Value);
    }

    private void RemoveActiveEffect(ActiveGameplayEffect activeEffect)
    {
        if (activeEffect == null)
            return;

        activeEffect.StopPeriodic(this);

        foreach (var tag in activeEffect.effect.grantedTags)
            RemoveTag(tag);

        m_AttributeSet.RemoveEffectModifiers(activeEffect.effect);

        m_ActiveEffects.Remove(activeEffect.effect.GetInstanceID());
        onEffectRemoved?.Invoke(activeEffect.effect);
    }    

    private IEnumerator RemoveEffectAfterDuration(ActiveGameplayEffect activeEffect, float duration)
    {
        while (Time.time - activeEffect.startTime < duration)
            yield return null;

        if (m_ActiveEffects.TryGetValue(activeEffect.effect.GetInstanceID(), out var effect))
            RemoveActiveEffect(activeEffect);
    }
    #endregion

    #region Tag Operations
    public bool HasTag(GameplayTag tag)
    {
        return activeTags.Has(tag);
    }

    public bool HasAllTags(IEnumerable<GameplayTag> tags)
    {
        return activeTags.HasAll(tags);
    }

    public bool HasAnyTag(IEnumerable<GameplayTag> tags)
    {
        return activeTags.HasAny(tags);
    }

    public void AddTag(GameplayTag tag)
    {
        if (!HasTag(tag))
        {
            activeTags.Add(tag);
            onTagAdded?.Invoke(tag);
        }
    }

    public bool RemoveTag(GameplayTag tag)
    {
        bool removed = activeTags.Remove(tag);
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
