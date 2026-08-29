using UnityEngine;

public class PlayerStateAirborneCombat : PlayerStateAirborne
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.SetAnimationBool(AnimationConsts.doubleJump, false);
        m_Player.model.SetAnimationBool(AnimationConsts.airborneCombat, true);
    }

    public override bool Exit(StateBase newState)
    {
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(PlayerStateAirborneCombat)))
        {
            m_Player.model.SetAnimationBool(AnimationConsts.airborneCombat, false);
        }
        return base.Exit(newState);
    }

    protected void ApplyGravityRatioWhenAttackAirborne()
    {
        float vy = m_Player.verticalVelocity.y;

        if (vy <= 0f)
        {
            m_Player.rigidBody.AddForce(
                Physics.gravity * (m_Player.attackComponent.skill.airAttackFallGravityScale - 1f) * Time.deltaTime,
                ForceMode.VelocityChange);
        }
    }
}
