using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionController : MonoBehaviour
{
    // ------------------ Action Toggle Start ----------------------
    private WaitForEndOfFrame m_WaitForEndOfFrame = new WaitForEndOfFrame();
    private bool m_ShouldPlayerRun = true;
    private bool m_IsJumpPerformed = false;
    private bool m_IsRollPerformed = false;
    private bool m_IsLightAttackPerformed = false;
    private bool m_IsDefenceHold = false;
    private bool m_IsDodgePerformed = false;
    // ------------------ Action Toggle End ------------------------

    // ------------------ Camera Control Start ----------------------
    [Header("Camera Control")]
    [SerializeField] private Transform m_FollowTarget;
    [SerializeField] private float m_HorizontalRotationSpeed = 0.3f;
    [SerializeField] private float m_VerticalRotationSpeed = 0.12f;
    [Range(-45, 45)]
    // limit camera's vertical movement
    [SerializeField] private float m_BottomClamp = -20f;
    // limit camera's vertical movement
    [Range(0, 90)]
    [SerializeField] private float m_TopClamp = 35f;

    private float m_CinemachineTargetPitch = 0f;
    private float m_CinemachineTargetYaw = 0f;
    // ------------------ Camera Control End ----------------------
    
    public Vector2 cameraMovement => InputManager.instance.playerActions.CameraMove.ReadValue<Vector2>();
    public Quaternion cameraRotation => Quaternion.Euler(new Vector3(0f, Camera.main.transform.eulerAngles.y, 0f));
    public Vector3 cameraFwd => Camera.main.transform.forward;
    public bool isCameraMoving => cameraMovement != Vector2.zero;

    public Vector2 playerMovement => InputManager.instance.playerActions.Move.ReadValue<Vector2>();
    public bool shouldRun => m_ShouldPlayerRun;
    public bool isMoving => playerMovement != Vector2.zero;
    public bool isJump => m_IsJumpPerformed;
    public bool isRoll => m_IsRollPerformed;
    public bool isLightAttack => m_IsLightAttackPerformed;
    public bool isDodge => m_IsDodgePerformed;
    public bool holdDefence => m_IsDefenceHold;

    #region State Methods
    private void OnEnable()
    {
        InputManager.instance.playerActions.RunToggle.performed += OnSwitchRunToggle;
        InputManager.instance.playerActions.Jump.performed += OnJumpPerformed;
        InputManager.instance.playerActions.Roll.performed += OnRollPerformed;
        InputManager.instance.playerActions.LightAttack.performed += OnLightAttackPerformed;
        InputManager.instance.playerActions.HoldDefence.performed += OnDefenceHold;
        InputManager.instance.playerActions.HoldDefence.canceled += OnDefenceCancel;
        InputManager.instance.playerActions.Dodge.performed += OnDodgePerformed;
    }

    private void OnDisable()
    {
        InputManager.instance.playerActions.RunToggle.performed -= OnSwitchRunToggle;
        InputManager.instance.playerActions.Jump.performed -= OnJumpPerformed;
        InputManager.instance.playerActions.Roll.performed -= OnRollPerformed;
        InputManager.instance.playerActions.LightAttack.performed -= OnLightAttackPerformed;
        InputManager.instance.playerActions.HoldDefence.performed -= OnDefenceHold;
        InputManager.instance.playerActions.HoldDefence.canceled -= OnDefenceCancel;
        InputManager.instance.playerActions.Dodge.performed -= OnDodgePerformed;
    }

    private void LateUpdate()
    {
        if (m_FollowTarget == null || !InputManager.instance.isEnabled)
            return;

        var input = cameraMovement;
        m_CinemachineTargetPitch = UpdateRotation(m_CinemachineTargetPitch, input.y, m_BottomClamp, m_TopClamp, true, m_VerticalRotationSpeed);
        m_CinemachineTargetYaw = UpdateRotation(m_CinemachineTargetYaw, input.x, float.MinValue, float.MaxValue, false, m_HorizontalRotationSpeed);
        ApplyRotations(m_CinemachineTargetPitch, m_CinemachineTargetYaw);
    }

    #endregion

    #region Main Methods
    public Vector3 GetInputDirection()
    {
        Vector3 move = Vector3.zero;
        Vector2 input = playerMovement;
        move.x = input.x;
        move.z = input.y;
        move = Vector3.ClampMagnitude(move, 1f);
        return move;
    }

    private float UpdateRotation(float current, float input, float min, float max, bool isXAxis, float speed)
    {
        current += (isXAxis ? -input : input) * speed;
        return Mathf.Clamp(current, min, max);
    }

    private void ApplyRotations(float pitch, float yaw)
    {
        m_FollowTarget.rotation = Quaternion.Euler(pitch, yaw, m_FollowTarget.eulerAngles.z);
    }
    #endregion

    #region Toggle Methods
    private void OnSwitchRunToggle(InputAction.CallbackContext context)
    {
        m_ShouldPlayerRun = !m_ShouldPlayerRun;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        m_IsJumpPerformed = true;
        MonoManager.Run(OnJumpCancel());
    }

    private void OnRollPerformed(InputAction.CallbackContext context)
    {
        m_IsRollPerformed = true;
        MonoManager.Run(OnRollCancel());
    }

    private void OnLightAttackPerformed(InputAction.CallbackContext context)
    {
        m_IsLightAttackPerformed = true;        
        MonoManager.Run(OnAttackCancel());     
    }

    private void OnDefenceHold(InputAction.CallbackContext context)
    {
        m_IsDefenceHold = true;
    }

    private void OnDefenceCancel(InputAction.CallbackContext context)
    {
        m_IsDefenceHold = false;
    }

    private void OnDodgePerformed(InputAction.CallbackContext context)
    {
        m_IsDodgePerformed = true;
        MonoManager.Run(OnDodgeCancel());
    }

    private IEnumerator OnJumpCancel()
    {
        yield return m_WaitForEndOfFrame;
        m_IsJumpPerformed = false;
    }

    private IEnumerator OnRollCancel()
    {
        yield return m_WaitForEndOfFrame;
        m_IsRollPerformed = false;
    }

    private IEnumerator OnAttackCancel()
    {
        yield return m_WaitForEndOfFrame;
        m_IsLightAttackPerformed = false;
    }

    private IEnumerator OnDodgeCancel()
    {
        yield return m_WaitForEndOfFrame;
        m_IsDodgePerformed = false;
    }
    #endregion
}
