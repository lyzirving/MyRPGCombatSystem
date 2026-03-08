
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

    public ChangeStateArgs(bool reEnterState)
    { 
        this.reEnterState = reEnterState;
        this.footStep = EFootstep.None;
    }

    public ChangeStateArgs(EFootstep footStep)
    {
        this.reEnterState = false;
        this.footStep = footStep;
    }
}

public interface IStateMachineOwner 
{
    public void ChangeState(ECharacterState state, ChangeStateArgs args = default(ChangeStateArgs));
}
