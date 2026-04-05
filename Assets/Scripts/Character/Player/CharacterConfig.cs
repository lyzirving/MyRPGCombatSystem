using System;
using UnityEngine;

[Serializable]
public class CharacterConfig
{
    [Header("Base Movement")]
    [Range(0f, 25f)] public float baseSpeed = 3f;
    [Range(0f, 1f)] public float walkSpeedModify = 0.4f;
    [Range(1f, 3f)] public float runSpeedModify = 1f;
    [Range(1f, 20f)] public float rotateSpeed = 8f;

    [Header("Jumpping Data")]
    public float jumpHeight = 1.5f;

    [Header("Falling Data")]
    [Range(1f, 5f)] public float fallGravityRatio = 1.2f;
}
