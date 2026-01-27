using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillConfig", menuName = "Config/SkillConfig")]
public class SkillConfig : ScriptableObject
{
    public string animationName;
    public SkillReleaseData releaseData;
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
