using UnityEngine;

public interface ISkillOwner
{
    public void OnAttack(ISkillTarget target, Vector3 hitPos);
}
