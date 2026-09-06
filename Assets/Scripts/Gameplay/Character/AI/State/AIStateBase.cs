
using UnityEngine;

public class AIStateBase : CharacterStateBase
{
    protected AIController m_AIController;

    #region State Methods
    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        m_AIController = owner as AIController;
    }
    #endregion
}
