using System;
using System.Collections;
using UnityEngine;

public class MonoManager : SingletonMono<MonoManager>
{
    private Func<bool> m_HandleInputAction;
    private Action m_UpdateAction;
    private Action m_LateUpdateAction;
    private Action m_FixedUpdateAction;

    private Func<bool> m_AdditiveHandleInputAction;
    private Action m_AdditiveUpdateAction;
    private Action m_AdditiveLateUpdateAction;
    private Action m_AdditiveFixedUpdateAction;

    public static Coroutine Run(IEnumerator routine)
    {
        return instance.StartCoroutine(routine);
    }

    public static void Stop(Coroutine routine)
    {
        if (routine != null)
            instance.StopCoroutine(routine);
    }

    public void AddHandleInputListener(Func<bool> func)
    {
        m_HandleInputAction += func;
    }

    public void RmoveHandleInputListener(Func<bool> func)
    {
        m_HandleInputAction -= func;
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

    public void AddAdditiveHandleInputListener(Func<bool> func)
    {
        m_AdditiveHandleInputAction += func;
    }

    public void RmoveAdditiveHandleInputListener(Func<bool> func)
    {
        m_AdditiveHandleInputAction -= func;
    }

    public void AddAdditiveUpdateListener(Action action)
    {
        m_AdditiveUpdateAction += action;
    }

    public void RemoveAdditiveUpdateListener(Action action)
    {
        m_AdditiveUpdateAction -= action;
    }

    public void AddAdditiveLateUpdateListener(Action action)
    {
        m_AdditiveLateUpdateAction += action;
    }

    public void RemoveAdditiveLateUpdateListener(Action action)
    {
        m_AdditiveLateUpdateAction -= action;
    }

    public void AddAdditiveFixedUpdateListener(Action action)
    {
        m_AdditiveFixedUpdateAction += action;
    }

    public void RemoveAdditiveFixedUpdateListener(Action action)
    {
        m_AdditiveFixedUpdateAction -= action;
    }

    private void Update()
    {
        if(m_HandleInputAction == null || !m_HandleInputAction.Invoke())
            m_UpdateAction?.Invoke();

        if (m_AdditiveHandleInputAction == null || !m_AdditiveHandleInputAction.Invoke())
            m_AdditiveUpdateAction?.Invoke();
    }

    private void LateUpdate()
    {
        m_LateUpdateAction?.Invoke();
        m_AdditiveLateUpdateAction?.Invoke();
    }

    private void FixedUpdate()
    {
        m_FixedUpdateAction?.Invoke();   
        m_AdditiveFixedUpdateAction?.Invoke();
    }
}
