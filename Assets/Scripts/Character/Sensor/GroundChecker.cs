using UnityEngine;
using System;

/// <summary>
/// Ground detection driven by physics collision contacts instead of raycasts.
/// A contact whose normal faces up (within a slope limit) means the character is
/// standing on something - ground, a cube, an AI's head, a moving platform, etc.
/// This removes the layer-based "Walkable" assumption, so any collider the character
/// physically collides with can be stood on.
/// </summary>
[Serializable]
public class GroundChecker
{
    public delegate void TouchGroundNotify(Collider collider);
    public delegate void ExitGroundNotify();

    public float GROUND_SLOPE_LIMIT = 45f;

    private bool m_IsGrounded = false;

    // Contact collected during the current physics step.
    private bool m_GroundedThisStep = false;
    private Collider m_GroundCollider = null;

    private TouchGroundNotify m_TouchGroundNotify;
    private ExitGroundNotify m_ExitGroundNotify;

    public TouchGroundNotify onTouch
    {
        get => m_TouchGroundNotify;
        set => m_TouchGroundNotify = value;
    }

    public ExitGroundNotify onExit
    {
        get => m_ExitGroundNotify;
        set => m_ExitGroundNotify = value;
    }

    public bool isGrounded => m_IsGrounded;

    /// <summary>
    /// Call once per physics frame (in FixedUpdate, before the next physics step):
    /// resolves the grounded state from the previous step's contacts, fires onTouch/onExit,
    /// then resets the per-step flag.
    /// </summary>
    public void Tick()
    {
        if (m_IsGrounded != m_GroundedThisStep)
        {
            m_IsGrounded = m_GroundedThisStep;

            if (m_IsGrounded)
                m_TouchGroundNotify?.Invoke(m_GroundCollider);
            else
                m_ExitGroundNotify?.Invoke();
        }

        m_GroundedThisStep = false;
        m_GroundCollider = null;
    }

    /// <summary>
    /// Feed collision contacts (from CharacterSensor.OnCollisionStay). Any contact whose
    /// normal faces up marks the character as grounded this step.
    /// </summary>
    public void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);
            if (Vector3.Angle(Vector3.up, contact.normal) < GROUND_SLOPE_LIMIT)
            {
                m_GroundedThisStep = true;
                m_GroundCollider = collision.collider;
                return;
            }
        }
    }
}
