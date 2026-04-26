using UnityEngine;

[CreateAssetMenu(fileName = "CharacterJumpData", menuName = "Config/CharacterJumpData")]
public class CharacterJumpData : ScriptableObject
{
    [Range(0.2f, 10f)] public float normalHeight = 1.5f;
    [Range(1f, 5f)]    public float fallGravityRatio = 1.2f;
}
