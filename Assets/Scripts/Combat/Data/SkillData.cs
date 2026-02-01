using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Config/SkillData")]
public class SkillData : ScriptableObject
{
    [Tooltip("Name of the animation state")]
    public string animation;
    [Tooltip("Name of the attack hotspot, which should be mapped to the one on player")]
    public string attackHotspot;
    [Tooltip("Index of the attack hotspot, initialized after SkillData is configured on player")]
    public int hotspotIndex = -1;

    [Tooltip("Data to be spawned when the skill is released")]
    public SkillReleaseData releaseData;
    [Tooltip("Data to be spawned when the skill hits the target")]
    public SkillAttackData attackData;
}

/// <summary>
/// Effect spawned when the skill starts
/// </summary>
[Serializable]
public class SkillReleaseData
{
    public SkillSpawnObj spawnObj;
    public AudioClip audioClip;
}

/// <summary>
/// Effect spawned when the skill hits target
/// </summary>
[Serializable]
public class SkillAttackData
{
    public SkillSpawnObj spawnObj;
    public AudioClip audioClip;
}

[Serializable]
public class SkillSpawnObj
{
    public GameObject prefab;
    public AudioClip audioClip;
    public Vector3 position = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public float time;
}
