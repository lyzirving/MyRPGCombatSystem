using UnityEngine;

public enum PlayerState : uint
{ 
    Idle = 0,
    Walk,
    Run,
    Jump,
    Falling,
    Land,
    Stop
}

public class PlayerStateBase : StateBase
{
    protected PlayerController m_Player;

    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        m_Player = owner as PlayerController;
    }

    public bool CheckPlayerOnGround()
    {
        Ray ray = GetGroundCheckRay();
        float distance = GetGroundCheckDistance();
        if (!Physics.SphereCast(ray.origin, m_Player.character.radius, ray.direction, out RaycastHit hitInfo, distance, GameConsts.EnvLayer, QueryTriggerInteraction.Ignore))
            return false;

        float angle = Vector3.Angle(-hitInfo.normal, ray.direction);
        return angle < 5f;
    }

    protected void ApplyGravity(float ratio = 1f)
    {
        m_Player?.character.Move(new Vector3(0f, ratio * m_Player.config.gravity * Time.deltaTime, 0f));
    }    

    protected void DrawGroundCheckRay()
    {
        if (!m_Player.config.drawDebug)
            return;

        Ray ray = GetGroundCheckRay();
        float distance = GetGroundCheckDistance();
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance, Color.red);
    }

    private Ray GetGroundCheckRay()
    {
        Ray ray = new Ray();
        ray.origin = m_Player.transform.position + m_Player.transform.up * m_Player.character.center.y;
        ray.direction = -m_Player.transform.up;
        return ray;
    }

    private float GetGroundCheckDistance()
    {
        return m_Player.character.height * 0.5f + 0.05f;
    }
}
