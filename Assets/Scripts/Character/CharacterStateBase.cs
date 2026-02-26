using UnityEngine;

public class CharacterStateBase : StateBase
{
    protected virtual void OnContactGround(Collider collider) { }

    protected virtual void OnExitGround() { }
}
