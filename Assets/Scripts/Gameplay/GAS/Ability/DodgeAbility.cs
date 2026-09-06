
/// <summary>
/// DodgeAbility triggers a dodge on the player character. It owns the dodge's
/// "intent" (resolving the dodge direction from input + facing) and its
/// visual/audio feedback (ghost trail, radial blur, dodge sound).
/// The actual movement / animation behavior lives in PlayerStateDodge; this
/// ability only drives the state lifecycle and the feedback cues.
/// It requires Tag.locked to be activated to ensure that the player is in a
/// state where dodging is allowed.
/// </summary>
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class DodgeAbility : GameplayAbility
{
    // Radial blur feedback (cached from the Global Volume profile).
    private RadialBlurBlitVolumeComponent m_VolumeComp;
    private Tween m_RadialBlurTween;
    private float m_RadialBlurIntensity;

    protected override void OnAbilityActivated()
    {
        CacheVolumeComponent();
    }

    protected override void OnAbilityCanceled()
    {
        OnDodgeFeedbackExit();
        ChangeStateWhenExit();
    }

    protected override void OnAbilityEnded()
    {
        OnDodgeFeedbackExit();
        ChangeStateWhenExit();
    }

    protected override void OnAbilityPerformed()
    {
        var player = m_Character as PlayerController;
        if (player == null)
        {
            EndAbility();
            return;
        }

        MakeDodgeAction(player, player.action.PlayerMovement);
        m_Character.ChangeState(ECharacterState.Dodge);

        OnDodgeFeedbackEnter(player);
    }

    protected override void OnAbilityReEnter()
    {
    }

    protected override void OnAbilityUpdate(float deltaTime)
    {
        var state = m_Character.currentState as PlayerStateDodge;
        if (state == null)
        {
            EndAbility();
            return;
        }

        if (state.IsExpired())
            EndAbility();
    }

    private void ChangeStateWhenExit()
    {
        // Always return to Idle. If the player is still holding movement input,
        // LocomotionAbility will be re-activated next frame and decide the correct
        // locomotion mode (Move / StrafeMove / Sprint) based on current Tags and input.
        m_Character.ChangeState(ECharacterState.Idle);
    }

    #region Dodge Direction

    /// <summary>
    /// Resolves the dodge direction from the camera-relative input, taking the
    /// character's current facing into account so the dodge follows the character's
    /// orientation instead of assuming the camera is always behind it.
    /// </summary>
    private void MakeDodgeAction(PlayerController player, Vector2 input)
    {
        Vector3 moveDir = InputToWorldDirection(input);
        Vector3 fwd = player.transform.forward.NormalizeIgnoreY();
        Vector3 right = player.transform.right.NormalizeIgnoreY();

        float forwardAmount = Vector3.Dot(moveDir, fwd);
        float rightAmount = Vector3.Dot(moveDir, right);

        ECharacterDodgeAction action = ECharacterDodgeAction.Backward;
        if (rightAmount > 0.4f)
            action = ECharacterDodgeAction.Right;
        else if (rightAmount < -0.4f)
            action = ECharacterDodgeAction.Left;
        else if (forwardAmount > 0.4f)
            action = ECharacterDodgeAction.Forward;

        player.dodgeAction = action;
    }

    /// <summary>
    /// Converts a camera-relative movement input (x = strafe, y = forward) into
    /// a world-space horizontal direction using only the camera's yaw, so the
    /// result stays on the ground plane and is independent of camera pitch.
    /// </summary>
    private Vector3 InputToWorldDirection(Vector2 input)
    {
        Vector3 dir = new Vector3(input.x, 0f, input.y);

        var cam = Camera.main;
        if (cam != null)
        {
            dir = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f) * dir;
        }

        return dir;
    }

    #endregion

    #region Dodge Feedback

    private void OnDodgeFeedbackEnter(PlayerController player)
    {
        player.ghostTrail.BeginTrail();
        player.PlayOneShot(player.config.dodge.audio);
        OnRadialBlurEffectEnter(player);
    }

    private void OnDodgeFeedbackExit()
    {
        var player = m_Character as PlayerController;
        if (player != null)
            player.ghostTrail.EndTrail();

        OnRadialBlurEffectExit();
    }

    private void CacheVolumeComponent()
    {
        if (m_VolumeComp != null)
            return;

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

    private void OnRadialBlurEffectEnter(PlayerController player)
    {
        if (m_VolumeComp == null) return;

        m_VolumeComp.UpdateFocusCenter(Camera.main, player.modelTransform.position, GetRadialBlurDirection(player.dodgeAction));
        m_RadialBlurTween?.Kill();
        m_RadialBlurIntensity = 0f;
        m_RadialBlurTween = DOTween.To(() => m_RadialBlurIntensity,
            (value) => m_RadialBlurIntensity = value,
            1f, m_VolumeComp.duration.value)
            .SetEase(Ease.InSine)
            .OnUpdate(OnScreenRadialUpdate);
    }

    private void OnRadialBlurEffectExit()
    {
        if (m_VolumeComp == null) return;

        m_RadialBlurTween?.Kill();
        m_VolumeComp.intensity.value = m_RadialBlurIntensity = 0f;
    }

    private void OnScreenRadialUpdate()
    {
        if (m_VolumeComp == null) return;

        m_VolumeComp.intensity.value = m_RadialBlurIntensity;
    }

    private Vector2 GetRadialBlurDirection(ECharacterDodgeAction playerAction)
    {
        switch (playerAction)
        {
            case ECharacterDodgeAction.Left:
                return Vector2.left;
            case ECharacterDodgeAction.Forward:
                return Vector2.up;
            case ECharacterDodgeAction.Backward:
                return Vector2.down;
            default:
                return Vector2.right;
        }
    }

    #endregion
}
