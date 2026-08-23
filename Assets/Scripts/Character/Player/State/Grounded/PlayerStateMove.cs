using UnityEngine;

public class PlayerStateMove : PlayerStateLocomotion
{
    protected int m_CurrentLoop;
    protected float m_CurrentTime;

    /// <summary>
    /// Get current footstep when moving.
    /// The implementation is dependent on actually animation.
    /// </summary>
    public virtual EFootstep CurrentFootstep
    {
        get 
        {
            if(m_Player.model.animator.IsInTransition(0)) return EFootstep.None;
            float time = m_Player.model.animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
            return time < 0.5f ? EFootstep.LeftFootstep : EFootstep.RightFootstep;
        }
    }

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_CurrentLoop = 0;
        m_CurrentTime = 0f;
    }

    public override void Update()
    {
        GetCurrentAnimationTimeInfo(out int currentLoop, out float currentTime);
                
        UpdateFootStep(currentLoop, currentTime, m_CurrentLoop, m_CurrentTime);
        UpdateAnimationValue();

        m_CurrentTime = currentTime;
        m_CurrentLoop = currentLoop;
    }    

    public override void FixedUpdate()
    {
        if (!m_Player.action.isMoving)
            return;

        m_Player.attrs.speedModify = m_Player.action.shouldRun ? m_Player.config.move.runModify : m_Player.config.move.walkModify;
        Vector3 targetDir = m_Player.GetTargetDirection();

        m_Player.RotateToTargetDir(targetDir, m_Player.config.move.rotateSpeed);
        MoveImmediately(targetDir * m_Player.speedScaler);
    }

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Move;
    }

    #region Virtual Method
    protected virtual void UpdateAnimationValue()
    {
        float speed = m_Player.action.shouldRun ? 2f : 1f;
        m_Player.model.SetAnimationFloat(AnimationConsts.speed, speed, 0.1f, Time.deltaTime);

        Vector3 forward = m_Player.transform.forward;
        Vector3 targetDir = m_Player.GetTargetDirection();
        float angle = Mathf.Rad2Deg * Mathf.Acos(Mathf.Clamp(Vector3.Dot(forward, targetDir), -1f, 1f));
        float angular = Mathf.Clamp(angle / 60f, 0f, 1f);
        float sign = Mathf.Sign(Vector3.Cross(forward, targetDir).y);
        m_Player.model.SetAnimationFloat(AnimationConsts.angular, angular * sign, 0.1f, Time.deltaTime);
    }
    #endregion          
}
