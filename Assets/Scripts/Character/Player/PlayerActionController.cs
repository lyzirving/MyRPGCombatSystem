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
    private bool m_IsLightPunchPerformed = false;    
    // ------------------ Action Toggle End ----------------------

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

    public Vector2 playerMovement
    {
        get => InputManager.instance.playerActions.Move.ReadValue<Vector2>();
    }

    public Vector2 cameraMovement
    {
        get => InputManager.instance.playerActions.CameraMove.ReadValue<Vector2>();
    }

    public bool isCameraMoving
    {
        get => cameraMovement != Vector2.zero;
    }

    public bool shouldRun { get => m_ShouldPlayerRun; }
    public bool isMoving { get => playerMovement != Vector2.zero; }
    public bool isJump { get => m_IsJumpPerformed; } 
    public bool isRoll { get => m_IsRollPerformed; }
    public bool isLightPunch { get => m_IsLightPunchPerformed; }

    #region State Methods
    private void OnEnable()
    {
        InputManager.instance.playerActions.RunToggle.performed += OnSwitchRunToggle;
        InputManager.instance.playerActions.Jump.performed += OnJumpPerformed;
        InputManager.instance.playerActions.Roll.performed += OnRollPerformed;
        InputManager.instance.playerActions.LightPunch.performed += OnLightPunchPerformed;
    }

    private void OnDisable()
    {
        InputManager.instance.playerActions.RunToggle.performed -= OnSwitchRunToggle;
        InputManager.instance.playerActions.Jump.performed -= OnJumpPerformed;
        InputManager.instance.playerActions.Roll.performed -= OnRollPerformed;
        InputManager.instance.playerActions.LightPunch.performed -= OnLightPunchPerformed;
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

    private void OnLightPunchPerformed(InputAction.CallbackContext context)
    {
        m_IsLightPunchPerformed = true;
        MonoManager.Run(OnAttackCancel());
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
        m_IsLightPunchPerformed = false;
    }
    #endregion
}
