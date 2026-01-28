using UnityEngine;

public class PlayerStateLightAttack : PlayerStateCombat
{
    private bool m_ShouldTransit = false;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(m_Player.animConsts.lightAttackHash);
        m_Player.model.RegisterRootMotionAction(HandleRootMotion);
        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.AttackTransit, HandleAttackTransit);

        m_ShouldTransit = false;
    }

    public override void Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveAction(AnimationEventType.AttackTransit, HandleAttackTransit);
        m_Player.model.RemoveRootMotionAction(HandleRootMotion);
        m_Player.model.StopAnimation(m_Player.animConsts.lightAttackHash);
        base.Exit(newState);
    }

    public override void Update()
    {
        if (!m_ShouldTransit) 
            return;

        if (InputManager.instance.isPlayerAttackPerformed)
        {
            ChangeStateArgs.Builder builder = new ChangeStateArgs.Builder();
            builder.Refresh(true);   
            m_Player.ChangeState(EPlayerState.LightAttack, builder.Build());
            return;
        }

        if (InputManager.instance.isPlayerRollPerformed)
        {
            m_Player.ChangeState(EPlayerState.Roll);
            return;
        }

        if (InputManager.instance.isPlayerJumpPerformed)
        {
            m_Player.ChangeState(EPlayerState.Jump);
            return;
        }

        if (InputManager.instance.isPlayerMoving)
        {
            m_Player.ChangeState(InputManager.instance.shouldPlayerRun ? EPlayerState.Run : EPlayerState.Walk);
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
        m_Player.transform.Translate(deltaPosition, Space.World);
    }

    private void HandleAttackTransit(in AnimationEventInfo info)
    {
        m_ShouldTransit = true;
    }
}
