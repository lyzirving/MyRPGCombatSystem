using UnityEngine;

public class PlayerStateAttack : PlayerStateCombat
{
    private float m_NormalizedTime = 0;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.attackComponent.skill.animatorState, m_Player.attackComponent.skill.crossFadeInTime);
        m_NormalizedTime = 0f;
        AnimationEventReceiver.instance.RegisterAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackCombo, HandleAttackCombo);        
    }

    public override void ReEnter(ChangeStateArgs args)
    {
        m_Player.model.StartAnimation(m_Player.attackComponent.skill.animatorState, m_Player.attackComponent.skill.crossFadeInTime);
        m_NormalizedTime = 0f;
    }

    public override bool Exit(StateBase newState)
    {
        m_Player.attackComponent.EndCombo();
        AnimationEventReceiver.instance.RemoveAction(GUIDConsts.PlayerAnimation, AnimationEventType.AttackCombo, HandleAttackCombo);
        base.Exit(newState);
        return true;
    }

    public override void Update()
    {
        if(!IsExpired())
            m_Player.model.animator.GetTargetAnimationTime(m_Player.attackComponent.skill.animatorState, AnimationConsts.BASE_LAYER, out m_NormalizedTime);           
    }

    public override void FixedUpdate()
    {
        m_Player.ResetHorizontalVelocity();
        Vector3 targetDir;
        if (m_Player.lockTarget != null && m_Player.sensor.distZone.IsZone(EDistanceZone.CloseCombatRange))
        {
            targetDir = m_Player.lockTarget.transform.position - m_Player.transform.position;
            targetDir = targetDir.NormalizeIgnoreY();
        }
        else
        {
            targetDir = m_Player.GetTargetDirection();
        }
        m_Player.RotateToTargetDir(targetDir, m_Player.config.move.rotateSpeed);
    }

    public override bool IsExpired()
    {
        return m_NormalizedTime >= m_Player.attackComponent.skill.transitionNormalizedTime;
    }  

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Attack;
    }

    private void HandleAttackCombo(in AnimationEventInfo info)
    {
        //[BugFix] fix animator graph doesn't sync with logic state
        if (info.animatorState != m_Player.attackComponent.skill.animatorState)
            return;

        m_Player.attackComponent.BeginCombo();
    }
}
