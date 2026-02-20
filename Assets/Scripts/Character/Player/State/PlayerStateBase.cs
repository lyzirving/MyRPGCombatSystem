
public class PlayerStateBase : StateBase
{    
    protected PlayerController m_Player;

    #region State Methods
    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        m_Player = owner as PlayerController;
    }
    #endregion
}
