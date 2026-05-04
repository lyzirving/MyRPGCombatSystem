using UnityEngine;

public interface ICharacterBehavior
{
    public bool isLightAttack { get; }
    public Transform modelTransform { get; }
    public int GUID { get; }
    public void OnAttackBegin();
    public void OnAttackEnd();
    public void OnAttackHit(ICharacterBehavior target, Vector3 hitPos);
    public void OnHit(Vector3 hitPos, in ICharacterBehavior source, in SkillData skillData);
    public void OnFootStep(EFootstep footStep);
    public void OnContactGround(Collider collider);
    public void OnExitGround();
    public void OnTargetFind(Transform target);
    public void OnTargetLost(Transform target);
    public void OnTargetChange(Transform current, Transform last);
    public void OnTargetDistZoneChange(EDistanceZone newZone, EDistanceZone oldZone, float distance);
}
