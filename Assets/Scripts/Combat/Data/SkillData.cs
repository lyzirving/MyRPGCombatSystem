using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Config/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("Animation Attributes")]
    [Tooltip("Name of the animator state")]
    public string animatorState;
    public float crossFadeInTime = 0.15f;
    [Tooltip("Input floating window duration in seconds")]
    public float inputWindowDuration = 0.2f;
    [Tooltip("Time to end current animation and transfer to another state")]
    public float transitionNormalizedTime = 1f;

    [Header("Trigger Attributes")]
    [Tooltip("Name of the attack box, which should be mapped to the one on player")]
    public string attackBox;
    public CombatDefine.EAttack action = CombatDefine.EAttack.None;

    [Header("Static Attributes")]
    public int damage = 5;
    public float hitStunTime = 0f;
    public float knockbackDistance = 0f;

    [Header("Spawner Data")]
    [Tooltip("Data to be spawned when the skill is released")]
    public SkillReleaseData skillReleaseData;
    [Tooltip("Data to be spawned when the skill hits the target")]
    public SkillHitData skillHitData;

    [Header("Runtime Attributes")]
    [Tooltip("Index of the attack box, initialized after SkillData is configured on player")]
    public int attackBoxIndex = -1;
    [Tooltip("Index of next skill in combo, initialized after SkillData is configured on player")]
    public int nextSkillIndex = -1;    
}

/// <summary>
/// Effect spawned when the skill starts
/// </summary>
[Serializable]
public class SkillReleaseData
{
    public string spawnPrefab;
    public AudioClip audioClip;
}

/// <summary>
/// Effect spawned when the skill hits target
/// </summary>
[Serializable]
public class SkillHitData
{
    public string spawnPrefab;
    public AudioClip audioClip;
    public float hitStopTimeScale;
}
