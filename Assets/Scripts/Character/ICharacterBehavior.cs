using UnityEngine;

public interface ICharacterBehavior
{
    public bool isLightAttack { get; }
    public void OnAttackBegin();
    public void OnAttackEnd();
    public void OnAttackHit(ICharacterBehavior target, Vector3 hitPos);
    public void OnHit(Vector3 hitPos, float damage);
    public void OnFootStep(EFootstep footStep);
    public void OnContactGround(Collider collider);
    public void OnExitGround();
}
