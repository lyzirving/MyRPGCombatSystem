using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDodgeData", menuName = "Config/CharacterDodgeData")]
public class CharacterDodgeData : ScriptableObject
{
    public float leftScale = 3f;
    public float rightScale = 3f;
    public float forwardScale = 1f;
    public float backwardScale = 1f;

    public float forwardHeight = 0.3f;
    public float backwardHeight = 0.2f;

    public AudioClip audio;
}
