using UnityEngine;

public class PlayerStateBase : StateBase
{    
    public Vector3 playerHorizonVelocity 
    {
        get 
        {
            var velocity = m_Player.rigidBody.linearVelocity;
            velocity.y = 0f;
            return velocity;
        }
    }

    public Vector3 playerVerticalVelocity
    {
        get
        {
            return new Vector3(0f, m_Player.rigidBody.linearVelocity.y, 0f);
        }
    }

    public float movementSpeed
    {
        get { return m_Player.config.baseSpeed * m_Player.attrs.speedModify; }
    }

    public bool isMoveHorizontally
    {
        get
        {
            var vel = playerHorizonVelocity;
            return new Vector2(vel.x, vel.z).magnitude > Mathf.Epsilon;
        }
    }

    public bool isMovingUp
    {
        get
        {
            return m_Player.rigidBody.linearVelocity.y > 0f;
        }
    }

    protected PlayerController m_Player;

    #region State Methods
    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        m_Player = owner as PlayerController;
    }

    public override void HandleTriggerEnter(Collider other)
    {
        if (GameUtility.IsWalkableLayer(other.gameObject.layer))
        {            
            OnContactGround(other);
        }
    }

    public override void HandleTriggerExit(Collider other)
    {
        if (GameUtility.IsWalkableLayer(other.gameObject.layer))
        {
            OnExitGround(other);
        }
    }

    protected virtual void OnContactGround(Collider collider) { }

    protected virtual void OnExitGround(Collider collider) { }
    #endregion

    #region Main Methods

    protected void ResetVelocity()
    {
        m_Player.rigidBody.linearVelocity = Vector3.zero;
    }

    protected void ResetVerticalVelocity()
    {
        m_Player.rigidBody.linearVelocity = playerHorizonVelocity;
    }

    protected Vector3 GetTargetDirection()
    {
        if (!m_Player.action.isMoving)
            return m_Player.transform.forward;

        return GetCameraRotation() * GetInputDirection();
    }

    protected Vector3 GetCameraDirection()
    {
        Vector3 direction = Camera.main.transform.forward;
        direction.y = 0f;
        direction.Normalize();
        return direction;
    }

    protected void RotateToTargetDir(Vector3 targetDir)
    {
        m_Player.transform.rotation = Quaternion.Slerp(m_Player.transform.rotation,
            Quaternion.LookRotation(targetDir), Time.deltaTime * m_Player.config.rotateSpeed);
    }

    protected Vector3 GetInputDirection()
    {
        Vector3 move = Vector3.zero;
        Vector2 input = m_Player.action.playerMovement;
        move.x = input.x;
        move.z = input.y;
        move = Vector3.ClampMagnitude(move, 1f);
        return move;
    }

    protected Quaternion GetCameraRotation()
    {
        return Quaternion.Euler(new Vector3(0f, Camera.main.transform.eulerAngles.y, 0f));
    }
    #endregion
}
