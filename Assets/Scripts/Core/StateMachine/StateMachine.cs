using System;
using System.Collections.Generic;

public interface IStateMachineOwner { }

public class StateMachine
{
    private IStateMachineOwner m_Owner;
    private Dictionary<Type, StateBase> m_StateDic = new Dictionary<Type, StateBase>();
    private StateBase m_CurrentState;

    public bool hasState { get => m_CurrentState != null; }
    public Type currentStateType { get => m_CurrentState.GetType(); }    
    

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(IStateMachineOwner owner)
    {
        m_Owner = owner;
    }

    /// <summary>
    /// 切换状态
    /// </summary>
    /// <typeparam name="T">具体要切换到的状态类型</typeparam>
    /// <param name="reCurrstate">如果状态没变，是否需要刷新状态</param>
    /// <returns></returns>
    public bool ChangeState<T>(bool reCurrstate = false) where T : StateBase, new()
    {
        // 状态一致，并且不需要刷新状态，则不需要进行切换
        if (hasState && currentStateType == typeof(T) && !reCurrstate) 
            return false;

        OnCurrentStateExit();
        OnDispatchNewState<T>();        
        return true;
    }    

    /// <summary>
    /// 停止工作，释放资源
    /// </summary>
    public void Stop()
    {
        OnCurrentStateExit();

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

    private void OnCurrentStateExit()
    {
        if (m_CurrentState != null)
        {
            m_CurrentState.Exit();
            MonoManager.instance.RemoveUpdateListener(m_CurrentState.Update);
            MonoManager.instance.RemoveLateUpdateListener(m_CurrentState.LateUpdate);
            MonoManager.instance.RemoveFixedUpdateListener(m_CurrentState.FxiedUpdate);
            m_CurrentState = null;
        }
    }

    private void OnDispatchNewState<T>() where T : StateBase, new()
    {
        m_CurrentState = GetState<T>();
        m_CurrentState.Enter();
        MonoManager.instance.AddUpdateListener(m_CurrentState.Update);
        MonoManager.instance.AddLateUpdateListener(m_CurrentState.LateUpdate);
        MonoManager.instance.AddFixedUpdateListener(m_CurrentState.FxiedUpdate);
    }    
}
