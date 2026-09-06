using UnityEngine;

[CreateAssetMenu(fileName = "CharacterJumpData", menuName = "Config/CharacterJumpData")]
public class CharacterJumpData : ScriptableObject
{
    [Header("Vertical")]
    [Range(0.2f, 10f)] public float normalHeight = 1.5f;
    [Range(1f, 5f)] public float fallGravityRatio = 1.2f;

    [Header("Air Control")]
    [Tooltip("Air movement speed factor: air target speed = run speed × this factor (industry common 0.6~0.9)")]
    [Range(0.2f, 1.5f)] public float airControlFactor = 0.7f;

    [Tooltip("Air rotation speed: significantly lower than ground rotateSpeed to avoid instant turns mid-air")]
    [Range(0.5f, 20f)] public float airRotateSpeed = 3f;

    [Tooltip("Air horizontal acceleration (m/s²): max rate at which air speed converges toward the input target. Lower = floatier, higher = more responsive")]
    [Range(1f, 20f)] public float airAcceleration = 8f;

    [Tooltip("Max airborne time (s) before the jump force-ends as a fall. Anti-stuck safety net so the state machine can never lock up.")]
    [Range(1f, 5f)] public float maxAirborneTime = 3f;

    [Header("Double Jump")]
    [Tooltip("Whether the character can perform an extra jump while airborne.")]
    public bool allowDoubleJump = true;

    [Tooltip("Height of the double jump in meters.")]
    [Range(0.2f, 10f)] public float doubleJumpHeight = 1.5f;

    [Tooltip("Whether the double jump can be performed while falling.")]
    public bool allowDoubleJumpWhileFalling = true;

    [Header("Audio")]
    public AudioClip audio;
}
