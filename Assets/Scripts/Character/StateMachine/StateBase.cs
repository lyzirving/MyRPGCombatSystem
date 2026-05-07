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
    /// Called when entering current state again
    /// </summary>
    public virtual void ReEnter(ChangeStateArgs args) { }

    /// <summary>
    /// Called once every time when a state exits
    /// </summary>
    /// <param name="newState"></param>
    /// <returns>true, succeed to exit one state, otherwise, false.</returns>
    public virtual bool Exit(StateBase newState) { return true; }

    public virtual bool HandleInput() { return false;  }

    public virtual void Update() { }

    public virtual void LateUpdate() { }

    public virtual void FixedUpdate() { }    

    public virtual ECharacterAction GetCurrentAction() { return ECharacterAction.None; }
}
