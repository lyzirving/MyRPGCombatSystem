using UnityEngine;

public class AIController : CharacterControllerBase
{
    #region State Methods
    private void Awake()
    {
        base.Init();
    }
    #endregion

    #region IStateMachineOwner Methods
    public override void ChangeState(ECharacterState state, ChangeStateArgs args = default(ChangeStateArgs))
    {
    }
    #endregion

    #region ICharacterBehavior Methods
    public override void OnDamage(float damage)
    {
        Debug.LogWarning($"OnDamage: {damage}");
    }
    #endregion
}
