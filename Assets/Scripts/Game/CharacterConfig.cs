using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfig", menuName = "Config/CharacterConfig")]
public class CharacterConfig : ScriptableObject
{
    public float fallGravityRatio = 1.2f;

    public float jumpHeightLow = 1.5f;
    public float jumpHeightMid = 2f;
    public float jumpHeightMax = 2.5f;
}
