using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "SkillData", menuName = "Config/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("Animation Attributes")]
    [Tooltip("Name of the animator state")]
    public string animatorState;
    public float crossFadeInTime = 0.15f;

    [Header("Combo Attributes")]
    [Tooltip("The time after the ability is activated, during which the ability cannot be interrupted by other abilities.")]
    [Range(0f, 1f)] public float minInterruptNormalizedTime = 0.22f;    
    [Tooltip("Normalized time at which the player can start inputting the next combo attack.")]    
    [Range(0f, 1f)] public float comboWindowStartNormalizedTime = 0.35f;    
    [Tooltip("Time to end current animation and transfer to another state")]
    [Range(0f, 1f)] public float transitionNormalizedTime = 1f;
    [Tooltip("Input floating window duration in seconds")]
    public float inputWindowDuration = 0.2f;

    /// <summary>
    /// range of one animation normalized time
    /// 0.0                A (minInterruptNormalizedTime)   B (comboWindowStartNormalizedTime)               C (transitionNormalizedTime)      1.0
    //├────────────────────┼────────────────────────────────┼────────────────────────────────────────────────┼──────────────────────────────────┤
    //│ StartUp            │        Can be canceled         │    Combo input window(user input)              │                                  │
    //│ Can't be canceled  │                                │                                                │                                  │
    //│                    │                                │                                                │                                  │
    //|                    │                                │ <----- next skill's inputWindowDuration -----> |                                  |
    //└────────────────────┴────────────────────────────────┴────────────────────────────────────────────────┴──────────────────────────────────┴
    /// </summary>

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

    [NonSerialized] public bool isLoaded = false;

    public void Load(Transform root = null)
    {
        if (isLoaded)
            return;

        isLoaded = true;

        if (skillReleaseData != null && !string.IsNullOrEmpty(skillReleaseData.spawnPrefab))
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(skillReleaseData.spawnPrefab);
            handle.WaitForCompletion();
            if(handle.Result == null)
            {
                Debug.LogError($"SkillData Load: fail to load prefab[{skillReleaseData.spawnPrefab}]");
                return;
            }
            var vfxEffect = GameObject.Instantiate(handle.Result, root);
            vfxEffect.SetActive(false);
            skillReleaseData.effectInst = vfxEffect.GetComponent<VFXEffect>();
            if (skillReleaseData.effectInst != null)
            {
                skillReleaseData.effectInst.duration = skillReleaseData.vfxTime;
            }
            else
            {
                Debug.LogError($"SkillData Load: fail to get VFXEffect from [{skillReleaseData.spawnPrefab}]");
            }
        }
    }
}

/// <summary>
/// Effect spawned when the skill starts
/// </summary>
[Serializable]
public class SkillReleaseData
{
    public string spawnPrefab;
    public float vfxTime;
    public AudioClip audioClip;
    [NonSerialized] public VFXEffect effectInst;
}

/// <summary>
/// Effect spawned when the skill hits target
/// </summary>
[Serializable]
public class SkillHitData
{
    public string spawnPrefab;
    public AudioClip audioClip;
    [Tooltip("Animator speed multiplier during hit stop. 0 = complete freeze, 1 = no effect. Typical value: 0.05~0.2")]
    [Range(0f, 1f)] public float hitStopTimeScale = 0.1f;
    [Tooltip("Duration of hit stop in real-time seconds. Typical values: light attack 0.03~0.06s, heavy attack 0.08~0.15s")]
    [Range(0f, 0.5f)] public float hitStopDuration = 0.06f;
}
