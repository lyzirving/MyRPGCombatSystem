using System.Collections;
using UnityEngine;

public class PlayerStateLockedOnMove : PlayerStateMove
{
    private const float SHOULDER_OFFSET_RESUME_TIME = 0.3f;
    private const float SHOULDER_OFFSET_RUNNING_TIME = 0.6f;

    private float m_TargetYawVelocity;
    private float m_TargetShoulderOffsetVelocity;
    private bool m_IsTargetCentraling = false;
    private Coroutine m_Coroutine = null;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.StartAnimation(AnimationConsts.locked);
        m_TargetYawVelocity = 0;
        m_TargetShoulderOffsetVelocity = 0;
        m_IsTargetCentraling = false;        
    }

    public override bool Exit(StateBase newState)
    {
        base.Exit(newState);
        if (m_Player.action.baseShoulderOffset != m_Player.action.currentShoulderOffset)
        {
            if (m_Coroutine != null) MonoManager.Stop(m_Coroutine);

            m_Coroutine = MonoManager.Run(BackToBaseShoulderOffset());            
        }
        return true;
    }

    public override void Update()
    {
        if (m_Player.lockTarget == null)
        {
            m_Player.ChangeState(ECharacterState.Move);
            return;
        }

        if (m_Player.action.isMoving && !m_Player.IsDirectionInView(m_Player.action.cameraFwd))
        {
            m_Player.lockTarget = null;
            m_Player.ChangeState(ECharacterState.Move);
            return;
        }

        base.Update();
    }

    public override void FixedUpdate()
    {
        if (!m_Player.action.isMoving || m_Player.lockTarget == null)
            return;

        m_Player.attrs.speedModify = m_Player.config.runSpeedModify;

        Vector3 faceDir = m_Player.lockTarget.position - m_Player.transform.position;
        faceDir.y = 0;
        faceDir.Normalize();
        m_Player.RotateToTargetDir(faceDir, m_Player.config.rotateSpeed);

        Vector2 input = m_Player.action.playerMovement;
        Vector3 moveDir = m_Player.transform.right * input.x + m_Player.transform.forward * input.y;
        moveDir.y = 0;
        moveDir.Normalize();
        MoveImmediately(moveDir * m_Player.speedScaler);
    }

    public override void LateUpdate()
    {
        var input = m_Player.action.playerMovement;
        if (!Mathf.Approximately(input.x, 0f))
        {
            var offset = m_Player.action.baseShoulderOffset;
            offset.x = -Mathf.Sign(input.x);
            offset.x = Mathf.SmoothDamp(m_Player.action.currentShoulderOffset.x, offset.x, ref m_TargetShoulderOffsetVelocity, SHOULDER_OFFSET_RUNNING_TIME);
            m_Player.action.SetCameraShoulderOffset(offset);
        }

        Vector3 faceDir = m_Player.lockTarget.position - m_Player.transform.position;
        faceDir.y = 0;
        faceDir.Normalize();

        RecenteringTarget(faceDir);
    }

    protected override void UpdateAnimationValue()
    {
        Vector2 input = m_Player.action.playerMovement;
        m_Player.model.SetAnimationFloat(AnimationConsts.speed, input.y, 0.1f, Time.deltaTime);
        m_Player.model.SetAnimationFloat(AnimationConsts.angular, input.x, 0.1f, Time.deltaTime);
    }

    private void RecenteringTarget(Vector3 front)
    {
        float angle = Vector3.Angle(front, m_Player.action.cameraFwd);
        if (angle > m_Player.config.recenterStartAngle && !m_IsTargetCentraling)
        {
            m_IsTargetCentraling = true;
        }

        if (angle < m_Player.config.recenterStopAngle && m_IsTargetCentraling)
        {
            m_IsTargetCentraling = false;
        }

        if (m_IsTargetCentraling)
        {
            float target = m_Player.action.CalcCameraYaw(-front);
            m_Player.action.SetCameraYawSmoothDamp(target, ref m_TargetYawVelocity, m_Player.config.recenterDuration, Time.deltaTime);
        }
    }

    private IEnumerator BackToBaseShoulderOffset()
    {
        float velocity = 0f;
        float startTime = Time.time;
        while (!Mathf.Approximately(m_Player.action.currentShoulderOffset.x, m_Player.action.baseShoulderOffset.x))
        {
            var target = m_Player.action.baseShoulderOffset;
            target.x = Mathf.SmoothDamp(m_Player.action.currentShoulderOffset.x, target.x, ref velocity, SHOULDER_OFFSET_RESUME_TIME);
            m_Player.action.SetCameraShoulderOffset(target);
            yield return null;
        }
        m_Coroutine = null;
    }
}
