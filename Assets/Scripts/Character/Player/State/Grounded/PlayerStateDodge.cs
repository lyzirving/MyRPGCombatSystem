using UnityEngine;

public class PlayerStateDodge : PlayerStateLocomotion
{
    private EDodgeState m_State = EDodgeState.Start;
    private bool m_IsJumpPerformed = false;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.SetAnimationBool(AnimationConsts.dodge, true);
        m_Player.model.RegisterRootMotionAction(HandleRootMotion);
        m_State = EDodgeState.Start;
        m_IsJumpPerformed = false;
    }

    public override bool Exit(StateBase newState)
    {
        if(m_State != EDodgeState.Stop)
            return false;

        m_Player.ghostTrail.EndTrail();
        m_Player.model.SetAnimationFloat(AnimationConsts.angular, 0f);
        m_Player.model.SetAnimationFloat(AnimationConsts.verticalAngular, 0f);
        m_Player.model.SetAnimationBool(AnimationConsts.dodge, false);
        m_Player.model.RemoveRootMotionAction(HandleRootMotion);
        base.Exit(newState);
        return true;
    }

    public override void Update()
    {
        m_Player.model.animator.GetTargetAnimationTime("Dodge", AnimationConsts.BASE_LAYER, out float time);
        // Debug.Log($"PlayerStateDodge Update, SubState[{m_State}], time[{time}]");
        if (m_State == EDodgeState.Start)
        {            
            m_State = EDodgeState.Floating;
            SetAnimationValue(m_Player.dodgeAction);
            m_Player.ghostTrail.BeginTrail();
            m_Player.PlayOneShot(m_Player.config.dodge.audio);
        }
        else if (m_State == EDodgeState.Stop)
        {
            if (m_Player.lockTarget != null)
                m_Player.ChangeState(ECharacterState.StrafeMove);
            else
                m_Player.ChangeState(ECharacterState.Idle);
        }
        else if (m_State == EDodgeState.Floating)
        {            
            if (time >= 0.9f)
                m_State = EDodgeState.Stop;
        }
    }

    public override void FixedUpdate()
    {
        if (!m_IsJumpPerformed)
        {
            m_IsJumpPerformed = true;
            if (m_Player.dodgeAction == ECharacterDodgeAction.Forward)
                Jump(m_Player.config.dodge.forwardHeight);
            else if(m_Player.dodgeAction == ECharacterDodgeAction.Backward)
                Jump(m_Player.config.dodge.backwardHeight);
        }
    }

    public override ECharacterAction GetCurrentAction()
    {
        return ECharacterAction.Dodge;
    }

    private void HandleRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        switch (m_Player.dodgeAction)
        {
            case ECharacterDodgeAction.Forward:
                deltaPosition *= m_Player.config.dodge.forwardScale;
                deltaPosition = Vector3.Dot(m_Player.transform.forward, deltaPosition) * m_Player.transform.forward;
                m_Player.transform.Translate(deltaPosition, Space.World);
                break;
            case ECharacterDodgeAction.Backward:
                deltaPosition *= m_Player.config.dodge.backwardScale;
                deltaPosition = Vector3.Dot(-m_Player.transform.forward, deltaPosition) * -m_Player.transform.forward;
                m_Player.transform.Translate(deltaPosition, Space.World);
                break;
            case ECharacterDodgeAction.Left:
                deltaPosition *= m_Player.config.dodge.leftScale;
                deltaPosition = Vector3.Dot(-m_Player.transform.right, deltaPosition) * -m_Player.transform.right;
                m_Player.transform.Translate(deltaPosition, Space.World);
                break;
            case ECharacterDodgeAction.Right:
                deltaPosition *= m_Player.config.dodge.rightScale;
                deltaPosition = Vector3.Dot(m_Player.transform.right, deltaPosition) * m_Player.transform.right;
                m_Player.transform.Translate(deltaPosition, Space.World);
                break;
            default:
                break;
        }
    }

    private void SetAnimationValue(ECharacterDodgeAction action)
    {
        switch (action)
        {
            case ECharacterDodgeAction.Forward:
                m_Player.model.SetAnimationFloat(AnimationConsts.angular, 0f);
                m_Player.model.SetAnimationFloat(AnimationConsts.verticalAngular, 1f);
                break;
            case ECharacterDodgeAction.Backward:
                m_Player.model.SetAnimationFloat(AnimationConsts.angular, 0f);
                m_Player.model.SetAnimationFloat(AnimationConsts.verticalAngular, -1f);
                break;
            case ECharacterDodgeAction.Left:
                m_Player.model.SetAnimationFloat(AnimationConsts.angular, -1f);
                m_Player.model.SetAnimationFloat(AnimationConsts.verticalAngular, 0f);
                break;
            case ECharacterDodgeAction.Right:
                m_Player.model.SetAnimationFloat(AnimationConsts.angular, 1f);
                m_Player.model.SetAnimationFloat(AnimationConsts.verticalAngular, 0f);
                break;
            default:
                break;
        }
    }
}
