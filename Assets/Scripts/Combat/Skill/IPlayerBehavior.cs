using UnityEngine;

public interface IPlayerBehavior
{
    public void OnStartAttack(SkillConfig config);
    public void OnAttackHit(SkillConfig config, ISkillTarget target, Vector3 hitPos);
    public void OnStopAttack(SkillConfig config);
    public void OnFootStep();
}
