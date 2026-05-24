using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionController : MonoBehaviour
{
    private ICharacterBehavior m_CharacterBehavior;

    #region Action Toggle
    private bool m_ShouldPlayerRun = true;
    private bool m_IsDefenceHold = false;

    public Vector2 playerMovement => InputManager.instance.playerActions.Move.ReadValue<Vector2>();
    public bool shouldRun => m_ShouldPlayerRun;
    public bool isMoving => playerMovement != Vector2.zero;
    public bool isDefenceHolding => m_IsDefenceHold;
    #endregion

    #region Buffered Command
    [Header("Buffer Settings")]
    [SerializeField] private int maxCmdBufferSize = 5;
    [SerializeField] private float bufferWindow = 0.3f;
    private MaxHeap<BufferedCommand> m_BufferedCommand = new MaxHeap<BufferedCommand>();
    #endregion

    #region Camera Control
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

    public Vector2 cameraMovement => InputManager.instance.playerActions.CameraMove.ReadValue<Vector2>();
    public Quaternion cameraRotation => Quaternion.Euler(new Vector3(0f, Camera.main.transform.eulerAngles.y, 0f));
    public Vector3 cameraFwd => Camera.main.transform.forward;
    public bool isCameraMoving => cameraMovement != Vector2.zero;
    #endregion

    #region State Methods
    private void OnEnable()
    {
        // --------------- State Related Start --------------
        InputManager.instance.playerActions.RunToggle.performed += OnSwitchRunToggle;
        InputManager.instance.playerActions.HoldDefence.performed += OnDefenceHold;
        InputManager.instance.playerActions.HoldDefence.canceled += OnDefenceCancel;
        // --------------- State Related End ----------------

        // --------------- Event Related Start --------------
        InputManager.instance.playerActions.Jump.performed += OnJumpPerformed;
        InputManager.instance.playerActions.LightAttack.performed += OnLightAttackPerformed;        
        InputManager.instance.playerActions.Dodge.performed += OnDodgePerformed;
        // --------------- Event Related End ----------------
    }

    private void OnDisable()
    {
        InputManager.instance.playerActions.RunToggle.performed -= OnSwitchRunToggle;
        InputManager.instance.playerActions.HoldDefence.performed -= OnDefenceHold;
        InputManager.instance.playerActions.HoldDefence.canceled -= OnDefenceCancel;

        InputManager.instance.playerActions.Jump.performed -= OnJumpPerformed;
        InputManager.instance.playerActions.LightAttack.performed -= OnLightAttackPerformed;        
        InputManager.instance.playerActions.Dodge.performed -= OnDodgePerformed;
    }

    private void Update()
    {
        CheckBufferedCommand();
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
    public void Init(ICharacterBehavior characterBehavior)
    {
        m_CharacterBehavior = characterBehavior;
    }

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

    #region Buffered Command Methods
    private void EnqueueBufferedCommand(ECharacterAction action)
    {
        while (m_BufferedCommand.count >= maxCmdBufferSize)
        {
            m_BufferedCommand.Dequeue();
        }

        m_BufferedCommand.Enqueue(new BufferedCommand(action, BufferedCommand.Priority(action), bufferWindow));
    }

    private void CheckBufferedCommand()
    {
        // Only one cmd a frame. Note do not use while, it may cause infinite loop
        if (m_BufferedCommand.count > 0)
        {
            //Todo
        }
    }
    #endregion

    #region Toggle Methods
    private void OnSwitchRunToggle(InputAction.CallbackContext context)
    {
        m_ShouldPlayerRun = !m_ShouldPlayerRun;
    }

    private void OnDefenceHold(InputAction.CallbackContext context)
    {
        m_IsDefenceHold = true;
        m_CharacterBehavior.abilitySystemComp.TryActivateAbility<DefenceAbility>();
    }

    private void OnDefenceCancel(InputAction.CallbackContext context)
    {
        m_IsDefenceHold = false;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        m_CharacterBehavior.abilitySystemComp.TryActivateAbility<JumpAbility>();
    }

    private void OnLightAttackPerformed(InputAction.CallbackContext context)
    {
        m_CharacterBehavior.abilitySystemComp.TryActivateAbility<LightAttackAbility>();  
    }    

    private void OnDodgePerformed(InputAction.CallbackContext context)
    {
        m_CharacterBehavior.abilitySystemComp.TryActivateAbility<DodgeAbility>();
    }
    #endregion
}
