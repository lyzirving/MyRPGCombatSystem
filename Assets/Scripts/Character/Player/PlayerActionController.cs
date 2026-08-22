using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionController : MonoBehaviour
{
    private ICharacterBehavior m_CharacterBehavior;
    private LockTargetManager m_LockTargetManager;

    #region Action Toggle
    private bool m_ShouldPlayerRun = true;
    private bool m_IsDefenceHold = false;

    public Vector2 playerMovement => InputManager.instance.playerActions.Move.ReadValue<Vector2>();
    public bool shouldRun => m_ShouldPlayerRun;
    public bool isMoving => playerMovement != Vector2.zero;
    public bool isDefenceHolding => m_IsDefenceHold;
    #endregion

    #region Dodge / Sprint
    private float m_DodgeHoldStartTime = -1f;
    private const float DODGE_HOLD_THRESHOLD = 0.3f;

    /// <summary>
    /// True when Dodge key is held past the threshold while the player is moving,
    /// signaling that the character should transition into Sprint.
    /// </summary>
    public bool shouldSprint => isMoving && m_DodgeHoldStartTime > 0f
        && (Time.time - m_DodgeHoldStartTime) >= DODGE_HOLD_THRESHOLD;
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

        // --------------- Lock Target Start --------------
        InputManager.instance.playerActions.LockTarget.performed += OnLockTargetPerformed;
        InputManager.instance.playerActions.SwitchTarget.performed += OnSwitchTargetPerformed;
        // --------------- Lock Target End ----------------

        // --------------- Event Related Start --------------
        InputManager.instance.playerActions.Jump.performed += OnJumpPerformed;
        InputManager.instance.playerActions.LightAttack.performed += OnLightAttackPerformed;
        InputManager.instance.playerActions.HeavyAttack.performed += OnHeavyAttackPerformed;
        InputManager.instance.playerActions.Dodge.started += OnDodgePerformed;
        InputManager.instance.playerActions.Dodge.canceled += OnDodgeCanceled;        
        // --------------- Event Related End ----------------
    }

    private void OnDisable()
    {
        InputManager.instance.playerActions.RunToggle.performed -= OnSwitchRunToggle;
        InputManager.instance.playerActions.HoldDefence.performed -= OnDefenceHold;
        InputManager.instance.playerActions.HoldDefence.canceled -= OnDefenceCancel;

        InputManager.instance.playerActions.LockTarget.performed -= OnLockTargetPerformed;
        InputManager.instance.playerActions.SwitchTarget.performed -= OnSwitchTargetPerformed;

        InputManager.instance.playerActions.Jump.performed -= OnJumpPerformed;
        InputManager.instance.playerActions.LightAttack.performed -= OnLightAttackPerformed;
        InputManager.instance.playerActions.HeavyAttack.performed -= OnHeavyAttackPerformed;
        InputManager.instance.playerActions.Dodge.started -= OnDodgePerformed;
        InputManager.instance.playerActions.Dodge.canceled -= OnDodgeCanceled;
    }

    private void Update()
    {
        // order between buffered command and locomotion input is important: 

        // 1. check buffered command queue and execute the first valid command
        CheckBufferedCommand();

        // 2. check locomotion input and activate LocomotionAbility if needed
        CheckLocomotionInput();
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
        m_LockTargetManager = GetComponent<LockTargetManager>();
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
        if (m_BufferedCommand.count == 0) return;

        var cmd = m_BufferedCommand.Dequeue();
        if (cmd.IsExpired())
            return;

        ExecuteCommand(cmd.action);
    }

    private void ExecuteCommand(ECharacterAction action)
    {
        switch (action)
        {
            case ECharacterAction.Jump:
                m_CharacterBehavior.abilitySystemComp.TryActivateAbility<JumpAbility>();
                break;
            case ECharacterAction.LightAttack:
                ProcessAttackInput(CombatDefine.EAttack.LA);                
                break;
            case ECharacterAction.HeavyAttack:
                ProcessAttackInput(CombatDefine.EAttack.HA);   
                break;
            case ECharacterAction.Dodge:
                m_CharacterBehavior.abilitySystemComp.TryActivateAbility<DodgeAbility>();
                break;
            default:
                Debug.LogWarning($"Unhandled buffered command: {action}");
                break;
        }
    }      

    private void ProcessAttackInput(CombatDefine.EAttack inputType)
    {        
        var attackComponent = m_CharacterBehavior.attackComponent;
        var asc = m_CharacterBehavior.abilitySystemComp;

        // Soft→Hard auto upgrade: when attacking in soft-lock mode, promote the
        // current soft-lock target to a temporary hard lock for this attack.
        if (m_LockTargetManager != null)
        {
            var lockAbility = asc.GetActive<LockTargetAbility>();
            if (lockAbility != null && lockAbility.IsInSoftMode)
            {
                m_LockTargetManager.UpgradeSoftToHardLock();
            }
        }

        // Sprint → Attack: route to dedicated sprint combo sequence
        if (m_CharacterBehavior.stateMachine.currentState is PlayerStateSprint)
        {
            CombatDefine.EAttack sprintAttackType = inputType == CombatDefine.EAttack.LA
                ? CombatDefine.EAttack.SprintLA
                : CombatDefine.EAttack.SprintHA;

            int sprintComboIndex = attackComponent.FindComboIndexByStartAction(sprintAttackType);
            if (sprintComboIndex >= 0)
            {
                attackComponent.SetComboIndex(sprintComboIndex);
                asc.TryActivateAbility<AttackAbility>();
            }
            else
            {
                // No dedicated sprint attack configured — fall back to regular attack
                int fallbackIndex = attackComponent.FindComboIndexByStartAction(inputType);
                if (fallbackIndex >= 0)
                {
                    attackComponent.SetComboIndex(fallbackIndex);
                    asc.TryActivateAbility<AttackAbility>();
                }
            }
            return;
        }

        var currentAttack = asc.GetActive<AttackAbility>();
        if (currentAttack != null)
        {            
            // in the middle of a combo
            var attackState = m_CharacterBehavior.stateMachine.currentState as PlayerStateAttack;
            float curTime = attackState?.CurrentNormalizedTime ?? 0f; 

            // check if we can advance to the next skill
            if (attackComponent.TryAdvanceCombo(inputType, curTime))
            {                
                currentAttack.ReActivate(asc);
                return;
            }
                        
            if(!currentAttack.CanBeInterrupted(curTime))
            {                
                // if the current attack cannot be interrupted, we ignore the input
                currentAttack.CachePendingComboInput(inputType);
                return;
            }

            if (!attackComponent.isComboStart)
            {
                // Combo window hasn't opened yet (e.g. delayed by HitStop).
                // Cache the input so it can be consumed when the window opens.                
                currentAttack.CachePendingComboInput(inputType);
                return;
            }
            
            // break current combo
            currentAttack.EndAbility();
        }

        // start a new combo
        int comboIndex = attackComponent.FindComboIndexByStartAction(inputType);        
        if (comboIndex >= 0)
        {
            attackComponent.SetComboIndex(comboIndex);
            asc.TryActivateAbility<AttackAbility>();
        }
    }
    #endregion

    #region Toggle Methods
    private void OnSwitchRunToggle(InputAction.CallbackContext context)
    {
        m_ShouldPlayerRun = !m_ShouldPlayerRun;
    }

    private void OnLockTargetPerformed(InputAction.CallbackContext context)
    {        
        m_LockTargetManager?.ToggleLock();
    }

    private void OnSwitchTargetPerformed(InputAction.CallbackContext context)
    {
        // Read the right-stick (camera) direction at the time of button press.
        // If the stick is idle, SwitchTarget falls back to sequential cycling.
        Vector2 stickDir = cameraMovement;
        m_LockTargetManager?.SwitchTarget(stickDir);
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
        EnqueueBufferedCommand(ECharacterAction.Jump);
    }

    private void OnLightAttackPerformed(InputAction.CallbackContext context)
    {
        EnqueueBufferedCommand(ECharacterAction.LightAttack);        
    }  

    private void OnHeavyAttackPerformed(InputAction.CallbackContext context)
    {
        EnqueueBufferedCommand(ECharacterAction.HeavyAttack);
    }    

    private void OnDodgePerformed(InputAction.CallbackContext context)
    {
        m_DodgeHoldStartTime = Time.time;
    }

    private void OnDodgeCanceled(InputAction.CallbackContext context)
    {
        // Debug.Log($"Dodge key released. Hold duration: {Time.time - m_DodgeHoldStartTime:F2}s");
        if (m_DodgeHoldStartTime < 0f)
            return;

        float holdDuration = Time.time - m_DodgeHoldStartTime;
        m_DodgeHoldStartTime = -1f;

        if (holdDuration < DODGE_HOLD_THRESHOLD)
        {
            // Short press → Dodge
            EnqueueBufferedCommand(ECharacterAction.Dodge);
        }
        // Long press → Sprint transition is handled by LocomotionAbility.OnAbilityUpdate()
        // via shouldSprint, so we don't enqueue anything here.
    }
    #endregion

    #region Locomotion Input
    /// <summary>
    /// Drives the LocomotionAbility based on player movement input.
    /// Called every frame in Update().
    /// </summary>
    private void CheckLocomotionInput()
    {
        if (!isMoving)
            return;

        var asc = m_CharacterBehavior.abilitySystemComp;
        if (asc == null)
            return;

        var locomotion = asc.GetActive<LocomotionAbility>();
        if (locomotion == null)
        {
            // Only activate locomotion when the state machine is in a locomotion-friendly state.
            // Prevents re-activating locomotion while Attack/Dodge/Defence/Jump is active,
            // which would overwrite the combat state on the next frame.
            var currentState = m_CharacterBehavior.stateMachine.currentState;
            if (currentState is PlayerStateIdle
                || currentState is PlayerStateMove
                || currentState is PlayerStateStrafeMove
                || currentState is PlayerStateSprint)
            {
                asc.TryActivateAbility<LocomotionAbility>();
            }
        }
        // If already active, LocomotionAbility.OnAbilityUpdate handles mode switching.
    }
    #endregion
}
