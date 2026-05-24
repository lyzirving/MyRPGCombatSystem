using System.Collections;
using UnityEngine;

public class ActiveGameplayEffect
{
    public GameplayEffect effect;
    public float startTime;
    public float duration;
    public object source;
    public AbilitySystemComponent target;

    private Coroutine m_PeriodicCoroutine;

    public ActiveGameplayEffect(GameplayEffect effect, float startTime, object source, AbilitySystemComponent target)
    {
        this.effect = effect;
        this.startTime = startTime;
        this.source = source;
        this.target = target;
        this.duration = effect.durationType == EffectDurationType.Duration ? effect.duration : 0f;
    }

    public void StartPeriodic(MonoBehaviour mono = null)
    {        
        if(effect.durationType != EffectDurationType.Periodical)
            return;

        m_PeriodicCoroutine = mono?.StartCoroutine(PeriodicEffectRoutine()) ?? MonoManager.instance.StartCoroutine(PeriodicEffectRoutine());
    }

    public void StopPeriodic(MonoBehaviour mono = null)
    {
        if (m_PeriodicCoroutine != null)
        {
            if (mono != null)
                mono.StopCoroutine(m_PeriodicCoroutine);
            else
                MonoManager.instance.StopCoroutine(m_PeriodicCoroutine);

            m_PeriodicCoroutine = null;
        }
    }

    private IEnumerator PeriodicEffectRoutine()
    {
        if(target == null)
            yield break;

        bool takeEffect = true;

        while (true)
        {
            if (takeEffect)
            {
                target.ApplyInstantEffect(effect, source);
                startTime = Time.time;
                takeEffect = false;
            }
            takeEffect = Time.time - startTime >= duration;
        }
    }
}
