using UnityEngine;

public class EnemyController : MonoBehaviour, ISkillTarget
{
    public void OnDamage(int damage)
    {
        Debug.LogWarning($"OnDamage: {damage}");
    }
}
