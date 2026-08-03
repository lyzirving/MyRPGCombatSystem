using UnityEngine;

public class PlayerStateSprint : PlayerStateLocomotion
{
    private Vector3 m_SprintDirection;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);

        // Record the move direction at entry (camera-relative)
        m_SprintDirection = m_Player.GetTargetDirection();

        // Set animator sprint parameter
        m_Player.model.SetAnimationBool(AnimationConsts.sprint, true);

        // Sprint speed modifier — reads from config if available, else defaults to 3x
        m_Player.attrs.speedModify = m_Player.config.move.sprintModify;
    }

    public override bool Exit(StateBase newState)
    {
        m_Player.model.SetAnimationBool(AnimationConsts.sprint, false);
        return base.Exit(newState);
    }

    public override bool HandleInput()
    {
        // Release movement key while sprinting → transition to Idle
        if (!m_Player.action.isMoving)
        {
            m_Player.ChangeState(ECharacterState.Idle);
            return true;
        }

        return false;
    }

    public override void Update()
    {
        // Continuously update sprint direction based on input
        if (m_Player.action.isMoving)
        {
            m_SprintDirection = m_Player.GetTargetDirection();
        }

        // Animation parameters: sprint speed = 3, smooth damped
        m_Player.model.SetAnimationFloat(AnimationConsts.speed, 3f, 0.1f, Time.deltaTime);

        // Angular velocity for turning (same logic as Move)
        Vector3 forward = m_Player.transform.forward;
        float angle = Mathf.Rad2Deg * Mathf.Acos(
            Mathf.Clamp(Vector3.Dot(forward, m_SprintDirection), -1f, 1f));
        float angular = Mathf.Clamp(angle / 60f, 0f, 1f);
        float sign = Mathf.Sign(Vector3.Cross(forward, m_SprintDirection).y);
        m_Player.model.SetAnimationFloat(AnimationConsts.angular, angular * sign, 0.1f, Time.deltaTime);
    }

    public override void FixedUpdate()
    {
        // Rotate toward sprint direction
        m_Player.RotateToTargetDir(m_SprintDirection, m_Player.config.move.rotateSpeed);

        // Apply movement along sprint direction
        MoveImmediately(m_SprintDirection * m_Player.speedScaler);
    }

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Sprint;
    }
}