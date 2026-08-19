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
        }
        else if (m_State == EDodgeState.Floating && time >= 0.9f)
        {
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

    public override bool IsExpired()
    {
        return m_State == EDodgeState.Stop;
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
                {
                    var playerFwd = m_Player.transform.forward.NormalizeIgnoreY();
                    deltaPosition *= m_Player.config.dodge.forwardScale;                    
                    deltaPosition = Vector3.Dot(playerFwd, deltaPosition) * playerFwd;
                    m_Player.transform.Translate(deltaPosition, Space.World);
                    break;
                }
            case ECharacterDodgeAction.Backward:
                {
                    var playerFwd = m_Player.transform.forward.NormalizeIgnoreY();
                    deltaPosition *= m_Player.config.dodge.backwardScale;
                    deltaPosition = Vector3.Dot(-playerFwd, deltaPosition) * -playerFwd;
                    m_Player.transform.Translate(deltaPosition, Space.World);
                    break;
                }
            case ECharacterDodgeAction.Left:
                {
                    deltaPosition *= m_Player.config.dodge.leftScale;
                    deltaPosition = Vector3.Dot(-m_Player.transform.right, deltaPosition) * -m_Player.transform.right;
                    m_Player.transform.Translate(deltaPosition, Space.World);
                    break;
                }
            case ECharacterDodgeAction.Right:
                {
                    deltaPosition *= m_Player.config.dodge.rightScale;
                    deltaPosition = Vector3.Dot(m_Player.transform.right, deltaPosition) * m_Player.transform.right;
                    m_Player.transform.Translate(deltaPosition, Space.World);
                    break;
                }
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
