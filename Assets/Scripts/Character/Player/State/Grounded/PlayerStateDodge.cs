using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerStateDodge : PlayerStateLocomotion
{
    private float RADIAL_BLUR_DURATION = 0.2f;
    private float RADIAL_BLUR_DEST_INTENSITY = 0.8f;

    private ScreenRadialBlurVolumeComponent m_VolumeComp;
    private EDodgeState m_State = EDodgeState.Start;
    private bool m_IsJumpPerformed = false;
    private Tween m_ScreenRadialBlurTween;
    private float m_ScreenRadialBlurIntensity;

    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        GameObject globalVolumeObj = GameObject.Find("Global Volume");
        if (globalVolumeObj != null)
        {
            Volume volume = globalVolumeObj.GetComponent<Volume>();
            if (volume != null && volume.profile != null)
            {
                volume.profile.TryGet(out m_VolumeComp);
            }
        }
    }

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        base.Enter(exitState, args);
        m_Player.model.SetAnimationBool(AnimationConsts.dodge, true);
        m_Player.model.RegisterRootMotionAction(HandleRootMotion);
        m_State = EDodgeState.Start;
        m_IsJumpPerformed = false;
        m_ScreenRadialBlurIntensity = 0f;
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
        OnRadialBlurEffectExit();       
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
            OnRadialBlurEffectEnter();
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

    private void OnRadialBlurEffectEnter()
    {
        if(m_VolumeComp == null) return;

        m_ScreenRadialBlurTween?.Kill();

        m_ScreenRadialBlurTween = DOTween.To(() => m_ScreenRadialBlurIntensity, (value) => m_ScreenRadialBlurIntensity = value, 
            RADIAL_BLUR_DEST_INTENSITY, 
            RADIAL_BLUR_DURATION)
            .SetLoops(1)
            .SetEase(Ease.InSine)
            .OnUpdate(OnScreenRadialUpdate)
            .OnComplete(OnScreenRadialForwardComplete);
    }

    private void OnRadialBlurEffectExit()
    {
        if (m_VolumeComp == null) return;

        m_ScreenRadialBlurTween?.Kill();
        m_VolumeComp.intensity.value = m_ScreenRadialBlurIntensity = 0f;
    }

    private void OnScreenRadialUpdate()
    {        
        m_VolumeComp.intensity.value = m_ScreenRadialBlurIntensity;
    }

    private void OnScreenRadialForwardComplete()
    {
        m_ScreenRadialBlurTween = DOTween.To(() => m_ScreenRadialBlurIntensity, (value) => m_ScreenRadialBlurIntensity = value,
            0f, RADIAL_BLUR_DURATION)
            .SetLoops(1)
            .SetEase(Ease.InSine)
            .OnUpdate(OnScreenRadialUpdate);
    }
}
