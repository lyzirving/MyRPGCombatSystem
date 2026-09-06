using UnityEngine;

[CreateAssetMenu(fileName = "CharacterMoveData", menuName = "Config/CharacterMoveData")]
public class CharacterMoveData : ScriptableObject
{
    [Range(0f, 25f)]  public float baseSpeed = 3f;
    [Range(1f, 20f)]  public float rotateSpeed = 8f;

    [Range(0f, 1f)]    public float walkModify = 0.4f;
    [Range(0.5f, 3f)]  public float runModify = 1f;
    [Range(1f, 6f)]    public float dodgeModify = 2f;
    [Range(0.5f, 5f)] public float sprintModify = 1.3f;    
}
