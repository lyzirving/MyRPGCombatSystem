using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class AbilitySystemComponent : MonoBehaviour
{
    public event Action<GameplayAttribute> onAttributeChanged;
    public event Action<GameplayTag> onTagAdded;
    public event Action<GameplayTag> onTagRemoved;
    public event Action<GameplayEffect> onEffectApplied;
    public event Action<GameplayEffect> onEffectRemoved;

    [SerializeField] private List<GameplayAttribute> m_GrantedAttributes = new List<GameplayAttribute>();  
    [SerializeField] private GameplayAbilitySet m_GrantedAbility;
    [SerializeField] private CharacterControllerBase m_Character;        

    private Dictionary<int, ActiveGameplayEffect> m_ActiveEffects = new Dictionary<int, ActiveGameplayEffect>();
    private Dictionary<int, GameplayAbility> m_ActiveAbilities = new Dictionary<int, GameplayAbility>();
    private List<GameplayAbility> m_AbilitiesToBeRemove = new List<GameplayAbility>();
    private List<GameplayAbility> m_UpdateSnapshot = new List<GameplayAbility>();

    private GameplayAttributeSet m_AttributeSet = new GameplayAttributeSet();
    private GameplayTagContainer m_ActiveTags = new GameplayTagContainer();

    public IReadOnlyDictionary<int, GameplayAbility> activeAbilities => m_ActiveAbilities;
    public int[] activeTagIndice => m_ActiveTags.indices;

    private void Awake()
    {
        InitializeAttributes();
        InitializeAbilities();
    }

    private void Update()
    {
        UpdateAbilities();        
    }    

    #region Main Methods
    public void Reset()
    {
        RemoveAllActiveEffects();

        m_AttributeSet.ClearAllModifiers();

        m_ActiveTags.Clear();

        InitializeAttributes();
    }

    private void InitializeAttributes()
    {
        m_AttributeSet.Clear();
        foreach (var attr in m_GrantedAttributes)
            m_AttributeSet.Add(attr);
    }

    private void InitializeAbilities()
    {
        if (m_GrantedAbility == null)
            throw new Exception("GameplayAbilitySet hasn't been set yet.");

        foreach (var ability in m_GrantedAbility)
        {
            ability.Attach(m_Character);
            if (ability.activationPolicy == AbilityActivationPolicy.OnSpawn)
                TryActivateAbility(ability);
        }
    }
    #endregion    

    #region Ability Operations
    public void RegisterActiveAbility(GameplayAbility ability)
    {
        if (ability == null)
            return;

        m_ActiveAbilities[ability.classHash] = ability;
    }

    public T GetActiveAccuratly<T>() where T : GameplayAbility
    {
        if (m_ActiveAbilities.TryGetValue(AbilityHash<T>.classHash, out var ability))
            return ability as T;
        return null;
    }

    /// <summary>
    /// Get active ability by iteration. 
    /// Only a few abilities are active at the same time, so this method is safe to use.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetActive<T>() where T : GameplayAbility
    {
        foreach (var ability in m_ActiveAbilities.Values)
        {
            if (ability is T tAbility)
                return tAbility;
        }
        return null;
    }

    public bool HasAbility<T>() where T : GameplayAbility => m_GrantedAbility?.Has<T>() ?? false;

    public bool TryActivateAbility<T>(object target = null) where T : GameplayAbility
    { 
        if(!HasAbility<T>())
            return false;

        var ability = m_GrantedAbility.Get<T>();
        return TryActivateAbility(ability, target);
    }

    public bool TryActivateAbility(GameplayAbility ability, object target = null)
    {
        if (ability == null)
            return false;

        if (ability.isActive)
        {
            ability.ReActivate(this, target);
            return true;
        }

        return ability.Activate(this, target);
    }

    public bool CancelAbility<T>() where T : GameplayAbility
    {
        if (m_ActiveAbilities.TryGetValue(AbilityHash<T>.classHash, out var ability))
            return CancelAbility(ability);
        return false;
    }

    public bool CancelAbility(GameplayAbility ability)
    {
        if(ability == null || !ability.canBeCanceled)
            return false;

        ability.EndAbility(true);
        return true;
    }

    public void CancelAllAbilities()
    { 
        m_AbilitiesToBeRemove.Clear();
        foreach (var item in m_ActiveAbilities)
        {
            var ability = item.Value;
            if (ability.canBeCanceled)
            {
                m_AbilitiesToBeRemove.Add(ability);
                ability.EndAbility(true);
            }
        }

        foreach (var item in m_AbilitiesToBeRemove)
            m_ActiveAbilities.Remove(item.classHash);
    }

    public void OnAbilityEnded(GameplayAbility ability)
    { 
    }

    public void OnAbilityCanceled(GameplayAbility ability)
    {
    }

    private void UpdateAbilities()
    {
        m_AbilitiesToBeRemove.Clear();

        // Snapshot the values first: an ability's OnUpdate may activate another ability
        // (which mutates m_ActiveAbilities), so enumerating the live collection would throw
        // "Collection was modified".
        m_UpdateSnapshot.Clear();
        m_UpdateSnapshot.AddRange(m_ActiveAbilities.Values);

        foreach (var ability in m_UpdateSnapshot)
        {
            if (ability.isActive)
                ability.OnUpdate(Time.deltaTime);
            else
                m_AbilitiesToBeRemove.Add(ability);
        }

        foreach (var remove in m_AbilitiesToBeRemove)
        {
            m_ActiveAbilities.Remove(remove.classHash);
        }
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
        return m_ActiveTags.Has(tag);
    }

    public bool HasAllTags(IEnumerable<GameplayTag> tags)
    {
        return m_ActiveTags.HasAll(tags);
    }

    public bool HasAnyTag(IEnumerable<GameplayTag> tags)
    {
        return m_ActiveTags.HasAny(tags);
    }

    public void AddTag(GameplayTag tag)
    {
        if (!HasTag(tag))
        {
            m_ActiveTags.Add(tag);
            onTagAdded?.Invoke(tag);
        }
    }

    public bool RemoveTag(GameplayTag tag)
    {
        bool removed = m_ActiveTags.Remove(tag);
        if (removed)
        {
            onTagRemoved?.Invoke(tag);
        }
        return removed;
    }

    public void RemoveAllGrantedTags(GameplayAbility ability)
    {
        if (ability == null || ability.grantedTags == null || ability.grantedTags.Count == 0)
            return;

        for (int i = 0; i < ability.grantedTags.Count; i++)
            RemoveTag(ability.grantedTags[i]);
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
