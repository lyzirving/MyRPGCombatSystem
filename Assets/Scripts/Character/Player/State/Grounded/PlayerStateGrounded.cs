using UnityEngine;

public class PlayerStateGrounded : PlayerStateBase
{
    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_Player.model.StartAnimation(m_Player.animConsts.groundHash);
    }

    public override void Exit(StateBase newState)
    {
        if (newState != null && !newState.GetType().IsSubclassOf(typeof(PlayerStateGrounded)))
        {
            m_Player.model.StopAnimation(m_Player.animConsts.groundHash);
        }
    }

    protected override void OnExitGround(Collider collider)
    {
        m_Player.ChangeState(EPlayerState.Falling);
    }

    protected void Float()
    {
        Vector3 centerInWorldSpace = m_Player.resizableCapsule.center;
        var ray = new Ray(centerInWorldSpace, -m_Player.transform.up);

        if (Physics.Raycast(ray, out RaycastHit hit, m_Player.resizableCapsule.slopeData.floatRayDistance, GameConsts.WalkableLayer, QueryTriggerInteraction.Ignore))
        {
            float groundAngle = Vector3.Angle(hit.normal, -ray.direction);

            float distanceToFloatingPoint = m_Player.resizableCapsule.colliderData.centerInLocalSpace.y * m_Player.transform.localScale.y - hit.distance;
            if (Mathf.Approximately(distanceToFloatingPoint, 0f))
                return;

            float amountToLift = distanceToFloatingPoint * m_Player.resizableCapsule.slopeData.stepReachForce - playerVerticalVelocity.y;
            Vector3 liftForce = new Vector3(0f, amountToLift, 0f);
            m_Player.rigidBody.AddForce(liftForce, ForceMode.VelocityChange);
        }
    }
}
