using UnityEngine;

[CreateAssetMenu(fileName = "ComboSequence", menuName = "Config/ComboSequence")]
public class ComboSequence : ScriptableObject
{
    public SkillData[] skillConfigs;
}
