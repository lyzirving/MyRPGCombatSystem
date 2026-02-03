using UnityEngine;

public class PlayerStateStandardAttack : PlayerStateCombat
{
    private bool m_ShouldTransit = false;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.attackComponent.skill.animation, m_Player.attackComponent.skill.crossFadeInTime);
        m_Player.model.RegisterRootMotionAction(HandleRootMotion);

        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.AnimationTransit, HandleAttackTransit);
        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.AttackCombo, HandleAttackCombo);

        m_ShouldTransit = false;
    }

    public override void Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveAction(AnimationEventType.AnimationTransit, HandleAttackTransit);
        AnimationEventReceiver.instance.RemoveAction(AnimationEventType.AttackCombo, HandleAttackCombo);
        m_Player.model.RemoveRootMotionAction(HandleRootMotion);
        base.Exit(newState);
    }

    public override void Update()
    {
        if (m_Player.attackComponent.UpdateCombo())
        {
            return;
        }

        if (!m_ShouldTransit) 
            return;

        if (m_Player.action.isPlayerAttackPerformed)
        {
            ChangeStateArgs.Builder builder = new ChangeStateArgs.Builder();
            builder.Refresh(true);   
            m_Player.ChangeState(EPlayerState.StandardAttack, builder.Build());
            return;
        }

        if (m_Player.action.isPlayerRollPerformed)
        {
            m_Player.ChangeState(EPlayerState.Roll);
            return;
        }

        if (m_Player.action.isPlayerJumpPerformed)
        {
            m_Player.ChangeState(EPlayerState.Jump);
            return;
        }

        if (m_Player.action.isPlayerMoving)
        {
            m_Player.ChangeState(m_Player.action.shouldPlayerRun ? EPlayerState.Run : EPlayerState.Walk);
            return;
        }
        else
        {
            m_Player.ChangeState(EPlayerState.Idle);
            return;
        }
    }

    public override void FixedUpdate()
    {
        ResetVelocity();

        Float();
    }

    private void HandleRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        deltaPosition.x = 0f;
        deltaPosition.y = 0f;
        m_Player.transform.Translate(deltaPosition, Space.Self);
    }

    private void HandleAttackTransit(in AnimationEventInfo info)
    {
        m_ShouldTransit = true;
    }

    private void HandleAttackCombo(in AnimationEventInfo info)
    {
        m_Player.attackComponent.StartCombo();
    }
}
