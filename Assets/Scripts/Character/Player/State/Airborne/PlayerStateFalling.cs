using UnityEngine;

public class PlayerStateFalling : PlayerStateAirborne
{
    private Vector3 m_PositionOnEnter;
    private int m_AniHash;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    { 
        base.Enter(exitState, args);
        m_AniHash = args.footStep == EFootStep.LeftFootStep ? m_Player.animConsts.fallLeftHash : m_Player.animConsts.fallRightHash;
        m_Player.model.StartAnimation(m_AniHash);
        ResetVerticalVelocity();

        m_Player.resizableCapsule.StoreStepHeightPercent();
        // Don't override WalkableCheck
        m_Player.resizableCapsule.SetStepHeightPercent(0.1f);
        m_PositionOnEnter = m_Player.transform.position;
    }

    public override void Exit(StateBase newState)
    {
        m_Player.resizableCapsule.RestoreStepHeightPercent();
        m_Player.model.StopAnimation(m_AniHash);
        base.Exit(newState);
    }

    public override void FixedUpdate()
    {
        RotateToTargetDir(GetTargetDirection());
        LimitVerticalVelocity();
    }

    protected override void OnContactGround(Collider collider)
    {        
        float fallDistance = m_PositionOnEnter.y - m_Player.transform.position.y;
        m_Player.ChangeState(EPlayerState.Land);
    }

    private void LimitVerticalVelocity()
    {
        var vel = playerVerticalVelocity;
        if (vel.y >= -m_Player.config.fallSpeedLimit)
            return;

        // a force pointing to up 
        Vector3 limitedVelocity = new Vector3(0f, -m_Player.config.fallSpeedLimit - vel.y, 0f);
        m_Player.rigidBody.AddForce(limitedVelocity, ForceMode.VelocityChange);
    }
}
