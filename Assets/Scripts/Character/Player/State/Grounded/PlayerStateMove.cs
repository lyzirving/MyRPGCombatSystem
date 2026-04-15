using UnityEngine;

public class PlayerStateMove : PlayerStateLocomotion
{
    private int m_AnimLoopCnt;
    private float m_AnimTime;

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
        m_AnimLoopCnt = 0;
        m_AnimTime = 0f;
    }

    public override void Update()
    {
        int currentLoop = 0;
        float currentTime = 0f;
        if (m_Player.model.animator.IsInTransition(AnimationConsts.BASE_LAYER))
        {
            currentLoop = m_AnimLoopCnt;
            currentTime = m_AnimTime;
        }
        else
        {
            var state = m_Player.model.animator.GetCurrentAnimatorStateInfo(AnimationConsts.BASE_LAYER);
            currentLoop = Mathf.FloorToInt(state.normalizedTime);
            currentTime = state.normalizedTime % 1f;
        }

        if (m_Player.action.isLightAttack)
        {
            m_Player.ChangeState(ECharacterState.Attack);
            return;
        }        

        if (m_Player.action.isJump)
        {
            m_Player.ChangeState(ECharacterState.Jump, new ChangeStateArgs(currentTime < 0.5f ? EFootstep.LeftFootstep : EFootstep.RightFootstep));
            return;
        }

        if (!m_Player.action.isMoving)
        {
            m_Player.ChangeState(ECharacterState.Idle);
            return;
        }
        
        UpdateAnimationValue();
        UpdateFootStep(currentLoop, currentTime, m_AnimLoopCnt, m_AnimTime);

        m_AnimTime = currentTime;
        m_AnimLoopCnt = currentLoop;
    }    

    public override void FixedUpdate()
    {
        if (!m_Player.action.isMoving)
            return;

        m_Player.attrs.speedModify = m_Player.action.shouldRun ? m_Player.config.runSpeedModify : m_Player.config.walkSpeedModify;
        Vector3 targetDir = m_Player.GetTargetDirection();

        m_Player.RotateToTargetDir(targetDir, m_Player.config.rotateSpeed);
        MoveImmediately(targetDir * m_Player.speedScaler);
    }

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Move;
    }

    private void UpdateAnimationValue()
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

    private void UpdateFootStep(int currentLoop, float currtentTime, int lastLoop, float lastTime)
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
