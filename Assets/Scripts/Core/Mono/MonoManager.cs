using System;
using System.Collections;
using UnityEngine;

public class MonoManager : SingletonMono<MonoManager>
{
    private Action m_UpdateAction;
    private Action m_LateUpdateAction;
    private Action m_FixedUpdateAction;

    public static Coroutine Run(IEnumerator routine)
    {
        return instance.StartCoroutine(routine);
    }

    public void AddUpdateListener(Action action)
    {
        m_UpdateAction += action;
    }

    public void RemoveUpdateListener(Action action)
    {
        m_UpdateAction -= action;
    }

    public void AddLateUpdateListener(Action action)
    {
        m_LateUpdateAction += action;
    }

    public void RemoveLateUpdateListener(Action action)
    {
        m_LateUpdateAction -= action;
    }

    public void AddFixedUpdateListener(Action action)
    {
        m_FixedUpdateAction += action;
    }

    public void RemoveFixedUpdateListener(Action action)
    {
        m_FixedUpdateAction -= action;
    }

    private void Update()
    {
        m_UpdateAction?.Invoke();
    }

    private void LateUpdate()
    {
        m_LateUpdateAction?.Invoke();
    }

    private void FixedUpdate()
    {
        m_FixedUpdateAction?.Invoke();
    }
}
