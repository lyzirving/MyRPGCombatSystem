using UnityEngine;

[CreateAssetMenu(fileName = "ComboSequence", menuName = "Config/ComboSequence")]
public class ComboSequence : ScriptableObject
{
    public SkillData[] skillConfigs;
    public SkillData lastSkillConfig { get => skillConfigs[skillConfigs.Length - 1]; }
}
