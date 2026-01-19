using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private IStateMachineOwner m_Owner;
    private Dictionary<Type, StateBase> m_StateDic = new Dictionary<Type, StateBase>();
    private StateBase m_CurrentState;

    public StateBase currentState { get => m_CurrentState; }
    public bool hasState { get => m_CurrentState != null; }
    public Type currentStateType { get => m_CurrentState.GetType(); }     


    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(IStateMachineOwner owner)
    {
        m_Owner = owner;
    }

    public bool ChangeState<T>(ChangeStateArgs args = default(ChangeStateArgs)) where T : StateBase, new()
    {
        // 状态一致，并且不需要刷新状态，则不需要进行切换
        if (hasState && currentStateType == typeof(T) && !args.reCurrstate) 
            return false;

        var exitState = m_CurrentState;
        var newState = GetState<T>();
        OnStateExit(exitState, newState);
        OnStateEnter(exitState, newState, args);
        m_CurrentState = newState;     
        return true;
    }    

    /// <summary>
    /// 停止工作，释放资源
    /// </summary>
    public void Stop()
    {
        OnStateExit(m_CurrentState, null);
        m_CurrentState = null;

        foreach (var item in m_StateDic.Values)
        {
            item.UnInit();
        }

        m_StateDic.Clear();
    }

    private StateBase GetState<T>() where T : StateBase, new()
    {
        Type type = typeof(T);
        if (!m_StateDic.TryGetValue(type, out StateBase state))
        {
            state = new T();
            state.Init(m_Owner);
            m_StateDic.Add(type, state);
        }
        return state;
    }

    private void OnStateExit(StateBase exitState, StateBase newState)
    {
        if (exitState != null)
        {
            Debug.Log($"[{exitState.GetType()}] exit");
            exitState.Exit(newState);
            MonoManager.instance.RemoveUpdateListener(exitState.Update);
            MonoManager.instance.RemoveLateUpdateListener(exitState.LateUpdate);
            MonoManager.instance.RemoveFixedUpdateListener(exitState.FixedUpdate);
        }
    }

    private void OnStateEnter(StateBase exitState, StateBase newState, ChangeStateArgs args)
    {
        if (newState != null)
        {
            Debug.Log($"[{newState.GetType()}] enter");
            newState.Enter(exitState, args);
            MonoManager.instance.AddUpdateListener(newState.Update);
            MonoManager.instance.AddLateUpdateListener(newState.LateUpdate);
            MonoManager.instance.AddFixedUpdateListener(newState.FixedUpdate);
        }
    }  
}
