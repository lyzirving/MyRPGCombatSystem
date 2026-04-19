using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private IStateMachineOwner m_Owner;
    private Dictionary<Type, StateBase> m_StateDic = new Dictionary<Type, StateBase>();
    private List<AdditiveState> m_AdditiveState = new List<AdditiveState>();
    private StateBase m_CurrentState;

    public StateBase currentState { get => m_CurrentState; }

    public T GetCurrentState<T>() where T : StateBase
    {
        if (m_CurrentState == null) return null;
        return m_CurrentState as T;
    }

    public bool IsCurrentState<T>() where T : StateBase
    {
        if (m_CurrentState == null) return false;
        return m_CurrentState is T;
    }

    public void Init(IStateMachineOwner owner)
    {
        m_Owner = owner;
    }

    #region Method for Base State
    public bool ChangeState<T>(ChangeStateArgs args = default(ChangeStateArgs)) where T : StateBase, new()
    {
        var newState = GetState<T>();
        if (newState == m_CurrentState && m_CurrentState != null)
        {
            m_CurrentState.ReEnter(args);
            return true;
        }

        var exitState = m_CurrentState;
        if (!OnStateExit(exitState, newState))
            return false;

        OnStateEnter(exitState, newState, args);
        m_CurrentState = newState;
        return true;
    }    

    public void ExitCurrentState()
    {
        if(m_CurrentState == null) return;

        OnStateExit(m_CurrentState, null);
        m_CurrentState = null;
    }

    public void Stop()
    {
        OnStateExit(m_CurrentState, null);
        m_CurrentState = null;

        StopAllAdditiveState();

        foreach (var item in m_StateDic.Values)
        {
            item.UnInit();
        }

        m_StateDic.Clear();
    }
    #endregion

    #region Method for Additive State
    public void AddAdditive<T>(ChangeStateArgs args = default(ChangeStateArgs)) where T : AdditiveState, new()
    {
        var state = GetState<T>();
        var addtive = state as AdditiveState;

        if (addtive == null) return;

        for (var i = 0; i < m_AdditiveState.Count; i++)
        {
            if (m_AdditiveState[i] == addtive)
            {
                m_AdditiveState[i].OnReAttach(args);
                return;
            }
        }

        m_AdditiveState.Add(addtive);
        addtive.OnAttach(args);

        MonoManager.instance.AddAdditiveUpdateListener(addtive.Update);
        MonoManager.instance.AddAdditiveLateUpdateListener(addtive.LateUpdate);
        MonoManager.instance.AddAdditiveFixedUpdateListener(addtive.FixedUpdate);
    }

    public void RemoveAdditive<T>() where T : AdditiveState, new()
    {
        var state = GetState<T>();
        var addtive = state as AdditiveState;

        if (addtive == null) return;

        for (var i = 0; i < m_AdditiveState.Count; i++)
        {
            if (m_AdditiveState[i] == addtive)
            {
                m_AdditiveState.RemoveAt(i);
                MonoManager.instance.RemoveAdditiveUpdateListener(addtive.Update);
                MonoManager.instance.RemoveAdditiveLateUpdateListener(addtive.LateUpdate);
                MonoManager.instance.RemoveAdditiveFixedUpdateListener(addtive.FixedUpdate);
                addtive.OnDetach();
            }
        }
    }
    #endregion

    #region Main Method
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

    private bool OnStateExit(StateBase exitState, StateBase newState)
    {
        if (exitState != null)
        {
            //Debug.Log($"[{exitState.GetType()}] exit");
            if (exitState.Exit(newState))
            {
                MonoManager.instance.RemoveUpdateListener(exitState.Update);
                MonoManager.instance.RemoveLateUpdateListener(exitState.LateUpdate);
                MonoManager.instance.RemoveFixedUpdateListener(exitState.FixedUpdate);
                return true;
            }
            return false;
        }
        else
        {
            return true;
        }
    }

    private void OnStateEnter(StateBase exitState, StateBase newState, ChangeStateArgs args)
    {
        if (newState != null)
        {
            //Debug.Log($"[{newState.GetType()}] enter");
            newState.Enter(exitState, args);
            MonoManager.instance.AddUpdateListener(newState.Update);
            MonoManager.instance.AddLateUpdateListener(newState.LateUpdate);
            MonoManager.instance.AddFixedUpdateListener(newState.FixedUpdate);
        }
    }

    private void StopAllAdditiveState()
    {
        for (int i = 0; i < m_AdditiveState.Count; i++)
        { 
            var state = m_AdditiveState[i];
            state.OnDetach();

            MonoManager.instance.RemoveAdditiveUpdateListener(state.Update);
            MonoManager.instance.RemoveAdditiveLateUpdateListener(state.LateUpdate);
            MonoManager.instance.RemoveAdditiveFixedUpdateListener(state.FixedUpdate);
        }
        m_AdditiveState.Clear();
    }
    #endregion
}
