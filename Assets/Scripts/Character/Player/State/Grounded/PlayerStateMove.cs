using UnityEngine;

public class PlayerStateMove : PlayerStateLocomotion
{
    protected int m_CurrentLoop;
    protected float m_CurrentTime;

    public EFootstep currentFoopStep
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

    protected void GetCurrentAnimationTimeInfo(out int loop, out float time)
    {
        // Note: Only consider the case when entering the move animation.
        //       If character quits the animation, the Update() will not run.
        if (m_Player.model.animator.IsInTransition(AnimationConsts.BASE_LAYER))
        {
            var state = m_Player.model.animator.GetNextAnimatorStateInfo(AnimationConsts.BASE_LAYER);
            loop = Mathf.FloorToInt(state.normalizedTime);
            time = state.normalizedTime % 1f;
        }
        else
        {
            var state = m_Player.model.animator.GetCurrentAnimatorStateInfo(AnimationConsts.BASE_LAYER);
            loop = Mathf.FloorToInt(state.normalizedTime);
            time = state.normalizedTime % 1f;
        }
    }

    protected void UpdateFootStep(int currentLoop, float currtentTime, int lastLoop, float lastTime)
    {
        float time = Time.time;
        if (currentLoop != lastLoop && Mathf.Abs(currentLoop - lastLoop) == 1)
        {
            m_Player.OnFootStep(EFootstep.RightFootstep);
        }

        if (lastTime < 0.5f && currtentTime >= 0.5f && !Mathf.Approximately(lastTime, 0f))
        {
            m_Player.OnFootStep(EFootstep.LeftFootstep);
        }
    }  
}
