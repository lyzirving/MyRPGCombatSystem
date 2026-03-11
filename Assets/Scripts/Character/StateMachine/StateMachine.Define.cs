
using UnityEngine;

public struct ChangeStateArgs
{
    /// <summary>
    /// Whether we should refresh state if current state doesn't change.
    /// </summary>
    public bool reEnterState;
    /// <summary>
    /// Current footStep of animation
    /// </summary>
    public EFootstep footStep;
    public Vector3 hitPos;

    public ChangeStateArgs(bool reEnterState)
    { 
        this.reEnterState = reEnterState;
        this.footStep = EFootstep.None;
        this.hitPos = Vector3.zero;
    }

    public ChangeStateArgs(EFootstep footStep)
    {
        this.reEnterState = false;
        this.footStep = footStep;
        this.hitPos = Vector3.zero;
    }

    public ChangeStateArgs(bool reEnterState, Vector3 hitPos)
    {
        this.reEnterState = reEnterState;
        this.footStep = EFootstep.None;
        this.hitPos = hitPos;
    }
}

public interface IStateMachineOwner 
{
    public void ChangeState(ECharacterState state, ChangeStateArgs args = default(ChangeStateArgs));
    public void ExitCurrentState();
}
