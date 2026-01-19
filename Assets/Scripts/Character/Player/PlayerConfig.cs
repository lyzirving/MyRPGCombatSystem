using System;
using UnityEngine;

[Serializable]
public class PlayerConfig
{
    [Header("Base Movement")]
    [Range(0f, 25f)]
    public float baseSpeed = 3f;
    [Range(0f, 1f)]
    public float walkSpeedModify = 0.4f;
    [Range(1f, 2f)]
    public float runSpeedModify = 1f;
    [Range(1f, 20f)]
    public float rotateSpeed = 8f;

    [Header("Jumpping Data")]
    public Vector3 stationaryJumpForce = new Vector3(0, 5f, 0f);
    public Vector3 weakJumpForce = new Vector3(1, 5f, 1f);
    public Vector3 mediumJumpForce = new Vector3(3.5f, 5f, 3.5f);
    public Vector3 strongJumpForce = new Vector3(5f, 5f, 5f);
    public float jumpStartRatio = 1.3f;

    [Header("Falling Data")]
    [Range(1f, 15f)]
    public float fallSpeedLimit = 15f;

    [Header("Landing Data")]
    [Range(1f, 100f)]
    public float minuDistanceToBeConsiderHardFall = 3f;
}
