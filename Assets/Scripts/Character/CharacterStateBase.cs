using UnityEngine;

public class CharacterStateBase : StateBase
{
    public virtual void OnContactGround(Collider collider) { }

    public virtual void OnExitGround() { }
}
