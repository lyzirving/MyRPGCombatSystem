using UnityEngine;

public class PlayerStateJump : PlayerStateAirborne
{
    private int m_AnimHash;
    private bool m_ShouldTransit;
    private bool m_ShouldStartJump;
    private bool m_IsJumpTrigger;
    private EFootStep m_FootStep;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_FootStep = args.footStep;
        m_AnimHash = m_FootStep == EFootStep.LeftFootStep ? m_Player.animConsts.jumpStartLeftHash : m_Player.animConsts.jumpStartRightHash;
        m_Player.model.StartAnimation(m_AnimHash);
        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.JumpStart, HandleJumpStart);
        AnimationEventReceiver.instance.RegisterAction(AnimationEventType.JumpStartTransit, HandleJumpStartTransit);

        m_IsJumpTrigger = m_ShouldStartJump = m_ShouldTransit = false;        
    }

    public override void Exit(StateBase newState)
    {
        AnimationEventReceiver.instance.RemoveAction(AnimationEventType.JumpStart, HandleJumpStart);
        AnimationEventReceiver.instance.RemoveAction(AnimationEventType.JumpStartTransit, HandleJumpStartTransit);
        m_Player.resizableCapsule.RestoreStepHeightPercent();
        m_Player.model.StopAnimation(m_AnimHash);
        base.Exit(newState);
    }

    public override void Update()
    {
        if (m_ShouldTransit || ShouldTransitToJumpIdle())
        {
            if (!m_IsJumpTrigger)
            {
                if(InputManager.instance.isPlayerMoving)
                    m_Player.ChangeState(InputManager.instance.shouldPlayerRun ? EPlayerState.Run : EPlayerState.Walk);
                else
                    m_Player.ChangeState(EPlayerState.Idle);
            }
            else
            {
                ChangeStateArgs.Builder b = new ChangeStateArgs.Builder();
                b.Footstep(m_FootStep);
                m_Player.ChangeState(EPlayerState.JumpIdle, b.Build());
            }            
        }
    }

    public override void FixedUpdate()
    {
        if (!m_IsJumpTrigger && m_ShouldStartJump)
        {
            m_IsJumpTrigger = true;
            m_Player.resizableCapsule.StoreStepHeightPercent();
            m_Player.resizableCapsule.SetStepHeightPercent(0f);
            Jump();
        }
    }

    private void Jump()
    {        
        Vector3 jumpDirection = GetTargetDirection();
        Vector3 jumpForce = m_Player.attrs.jumpForce;

        jumpForce.x *= jumpDirection.x;
        jumpForce.z *= jumpDirection.z;

        ResetVelocity();

        m_Player.rigidBody.AddForce(jumpForce, ForceMode.VelocityChange);
    }

    private bool ShouldTransitToJumpIdle()
    {
        var center = m_Player.resizableCapsule.checkBoxCenter;
        var extents = m_Player.resizableCapsule.checkBoxExtents;
        var ray = new Ray(center, Vector3.down);
        return !Physics.Raycast(ray, out RaycastHit hit, extents.y * m_Player.config.jumpStartRatio, GameConsts.WalkableLayer, QueryTriggerInteraction.Ignore);
    }

    private void HandleJumpStart(AnimationEventInfo info)
    {
        m_ShouldStartJump = true;
        m_Player.OnFootStep();
    }

    private void HandleJumpStartTransit(AnimationEventInfo info)
    {
        m_ShouldTransit = true;
    }
}
