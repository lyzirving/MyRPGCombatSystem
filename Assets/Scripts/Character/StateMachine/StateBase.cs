using UnityEngine;

public abstract class StateBase
{
    public virtual void Init(IStateMachineOwner owner) { }

    public virtual void UnInit() { }

    /// <summary>
    /// Called once every time when a state enters
    /// </summary>
    public virtual void Enter(StateBase exitState, ChangeStateArgs args) { }

    /// <summary>
    /// Called once every time when a state exits
    /// </summary>
    public virtual void Exit(StateBase newState) { }

    public virtual void Update() { }

    public virtual void LateUpdate() { }

    public virtual void FixedUpdate() { }

    public virtual void HandleTriggerEnter(Collider other) { }    

    public virtual void HandleTriggerExit(Collider other) { }    
}
