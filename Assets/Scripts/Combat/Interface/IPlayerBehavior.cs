using UnityEngine;

public interface IPlayerBehavior
{
    public void OnStartAttack(SkillData config);
    public void OnAttackHit(SkillData config, ISkillTarget target, Vector3 hitPos);
    public void OnStopAttack(SkillData config);
    public void OnFootStep();
    public PlayerActionController PlayerAction();
}
