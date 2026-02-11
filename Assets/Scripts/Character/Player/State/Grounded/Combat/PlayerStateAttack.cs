using UnityEngine;

public class PlayerStateAttack : PlayerStateCombat
{
    private bool m_ShouldTransit = false;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.attackComponent.skill.animation, m_Player.attackComponent.skill.crossFadeInTime);

        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.AnimationTransit, HandleAttackTransit);
        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.AttackCombo, HandleAttackCombo);

        m_ShouldTransit = false;
    }

    public override void Exit(StateBase newState)
    {        
        AnimationEventReceiver.instance.RemoveAction(AnimationEventType.AnimationTransit, HandleAttackTransit);
        AnimationEventReceiver.instance.RemoveAction(AnimationEventType.AttackCombo, HandleAttackCombo);

        base.Exit(newState);
    }

    public override void Update()
    {
        if (m_Player.attackComponent.UpdateCombo())
        {
            m_Player.attackComponent.NextSkill();
            m_Player.ChangeState(EPlayerState.Attack, new ChangeStateArgs.Builder(true).Build());
            return;
        }

        if (!m_ShouldTransit) 
            return;

        // Change to another state
        if (m_Player.action.isLightPunch)
        {
            m_Player.ChangeState(EPlayerState.Attack, new ChangeStateArgs.Builder(true).Build());            
        }
        else if (m_Player.action.isRoll)
        {
            m_Player.ChangeState(EPlayerState.Roll);
        }
        else if (m_Player.action.isJump)
        {
            m_Player.ChangeState(EPlayerState.Jump);
        }
        else if (m_Player.action.isMoving)
        {
            m_Player.ChangeState(m_Player.action.shouldRun ? EPlayerState.Run : EPlayerState.Walk);
        }
        else
        {
            m_Player.ChangeState(EPlayerState.Idle);
        }

        // After quit the PlayerStateStandardAttack
        m_Player.attackComponent.EndCombo();
    }

    public override void FixedUpdate()
    {
        ResetVelocity();

        Float();

        RotateToTargetDir(GetCameraDirection());
    }

    private void HandleAttackTransit(in AnimationEventInfo info)
    {
        m_ShouldTransit = true;
    }

    private void HandleAttackCombo(in AnimationEventInfo info)
    {
        m_Player.attackComponent.BeginCombo();
    }
}
