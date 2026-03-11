using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Config/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("Static Attributes")]
    [Tooltip("Name of the animation state")]
    public string animation;
    public float crossFadeInTime = 0.15f;
    [Tooltip("Input floating window duration in seconds")]
    public float inputWindowDuration = 0.2f;
    [Tooltip("Name of the attack box, which should be mapped to the one on player")]
    public string attackBox;
    public CombatDefine.EAttack action = CombatDefine.EAttack.None;

    public int damage = 5;

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
