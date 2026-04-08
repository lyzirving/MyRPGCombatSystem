using UnityEngine;

public class AdditiveState : StateBase
{
    public virtual void OnAttach(ChangeStateArgs args = default(ChangeStateArgs)) { }

    public virtual void OnReAttach(ChangeStateArgs args = default(ChangeStateArgs)) { }

    public virtual void OnDetach() { }
}
