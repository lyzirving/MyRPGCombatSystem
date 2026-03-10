using UnityEngine;

public class PlayerStateAttack : PlayerStateCombat
{
    private bool m_ShouldTransit = false;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.attackComponent.skill.animation, m_Player.attackComponent.skill.crossFadeInTime);

        AnimationEventReceiver.instance.RegisterAction(GUIDConsts.PlayerAnimation, AnimationEventType.AnimationTransit, HandleAttackTransit);
        AnimationEventReceiver.instance.RegisterAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackCombo, HandleAttackCombo);

        m_ShouldTransit = false;
    }

    public override void ReEnter(ChangeStateArgs args)
    {
        m_Player.model.StartAnimation(m_Player.attackComponent.skill.animation, m_Player.attackComponent.skill.crossFadeInTime);
        m_ShouldTransit = false;
    }

    public override void Exit(StateBase newState)
    {        
        AnimationEventReceiver.instance.RemoveAction(GUIDConsts.PlayerAnimation, AnimationEventType.AnimationTransit, HandleAttackTransit);
        AnimationEventReceiver.instance.RemoveAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackCombo, HandleAttackCombo);

        base.Exit(newState);
    }

    public override void Update()
    {
        if (m_Player.attackComponent.UpdateCombo())
        {
            m_Player.attackComponent.NextSkill();
            m_Player.ChangeState(ECharacterState.Attack, new ChangeStateArgs(true));
            return;
        }

        if (!m_ShouldTransit) 
            return;

        // Change to another state
        if (m_Player.action.isLightAttack)
        {
            m_Player.ChangeState(ECharacterState.Attack, new ChangeStateArgs(true));            
        }
        else if (m_Player.action.isJump)
        {
            m_Player.ChangeState(ECharacterState.Jump);
        }
        else if (m_Player.action.isMoving)
        {
            m_Player.ChangeState(ECharacterState.Move);
        }
        else
        {
            m_Player.ChangeState(ECharacterState.Idle);
        }

        // After quit the PlayerStateStandardAttack
        m_Player.attackComponent.EndCombo();
    }

    public override void FixedUpdate()
    {
        m_Player.ResetHorizontalVelocity();
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
