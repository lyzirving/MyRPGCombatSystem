using UnityEngine;

/// <summary>
/// 状态基类
/// </summary>
public abstract class StateBase
{
    /// <summary>
    /// 初始化
    /// 只在状态第一次创建的时候调用
    /// </summary>
    /// <param name="owner">宿主</param>
    /// <param name="stateType">当前这个状态代表的实际枚举的int值</param>
    public virtual void Init(IStateMachineOwner owner) { }

    /// <summary>
    /// 反初始化
    /// 销毁时释放一些资源
    /// </summary>
    public virtual void UnInit() { }

    /// <summary>
    /// 状态每次进入都执行一次
    /// </summary>
    public virtual void Enter(StateBase exitState, in ChangeStateArgs args) { }

    /// <summary>
    /// 状态退出时执行一次
    /// </summary>
    public virtual void Exit(StateBase newState) { }

    public virtual void Update() { }

    public virtual void LateUpdate() { }

    public virtual void FxiedUpdate() { }

    public virtual void HandleTriggerEnter(Collider other) { }

    public virtual void HandleTriggerExit(Collider other) { }
}
