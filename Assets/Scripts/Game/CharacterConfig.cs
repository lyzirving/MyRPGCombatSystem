using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfig", menuName = "Config/CharacterConfig")]
public class CharacterConfig : ScriptableObject
{
    public float fallGravityRatio = 1.2f;

    public float idleJumpHeight = 1.5f;
    public float runJumpHeight = 2f;
}
