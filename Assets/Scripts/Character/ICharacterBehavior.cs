using UnityEngine;

public interface ICharacterBehavior
{
    public bool isLightAttack { get; }
    public void OnAttackBegin();
    public void OnAttackEnd();
    public void OnAttackHit(SkillData config, ICharacterBehavior target, Vector3 hitPos);
    public void OnDamage(float damage);
    public void OnFootStep(EFootStep footStep);
    public void OnContactGround(Collider collider);
    public void OnExitGround();
}
