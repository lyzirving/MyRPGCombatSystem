using UnityEngine;

public delegate void AttackHitFunc(ISkillTarget target, Vector3 hitPos);

public interface ISkillTarget
{
    public void OnDamage(int damage);
}
