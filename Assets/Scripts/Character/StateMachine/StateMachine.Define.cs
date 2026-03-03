
public struct ChangeStateArgs
{
    /// <summary>
    /// Whether we should refresh state if current state doesn't change.
    /// </summary>
    public bool reEnterState;

    public ChangeStateArgs(bool reEnterState)
    { 
        this.reEnterState = reEnterState;
    }
}

public interface IStateMachineOwner 
{
    public void ChangeState(ECharacterState state, ChangeStateArgs args = default(ChangeStateArgs));
}
