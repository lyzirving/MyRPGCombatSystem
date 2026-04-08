using UnityEngine;

/// <summary>
/// Final Animation = Base Layer + Additive Layer(Current Pose - Reference Pose)
/// Reference Pose is by default the first frame of the clip.
/// 
/// AIStateHurt is used as AddtiveState in StateMachine
/// </summary>
public class AIStateHurt : AIStateGround
{
    private const float TRANSITION_INTERVAL = 0.9f;

    private float m_HitStunTime;
    private float m_EnterTime;
    private float m_StunningTime;
    private float m_KnockbackDistance;
    private ICharacterBehavior m_Source;

    public override void OnAttach(ChangeStateArgs args)
    {
        m_HitStunTime = args.skillData?.hitStunTime ?? 0f;
        m_KnockbackDistance = args.skillData?.knockbackDistance ?? 0f;
        m_Source = args.source;
        m_EnterTime = Time.time;
        m_StunningTime = 0f;

        m_AIController.model.StartAnimation(AnimationConsts.hit);
        m_AIController.model.SetAnimationFloat(AnimationConsts.hitTween, CalcHitTween(args.hitPos));
        m_AIController.model.SetAnimationFloat(AnimationConsts.hitStunning, HasHitStunning() ? 1.1f : 0f);
    }

    public override void OnReAttach(ChangeStateArgs args)
    {
        m_HitStunTime = args.skillData?.hitStunTime ?? 0f;
        m_KnockbackDistance = args.skillData?.knockbackDistance ?? 0f;
        m_Source = args.source;
        m_EnterTime = Time.time;
        m_StunningTime = 0f;

        m_AIController.model.StartAnimation(AnimationConsts.hurtState, 0.01f, AnimationConsts.HURT_LAYER);
        m_AIController.model.SetAnimationFloat(AnimationConsts.hitTween, CalcHitTween(args.hitPos));
        m_AIController.model.SetAnimationFloat(AnimationConsts.hitStunning, HasHitStunning() ? 1.1f : 0f);
    }

    public override void OnDetach()
    {
        m_AIController.model.StopAnimation(AnimationConsts.hit);
    }

    public override void Update()
    {
        float deltaTime = Time.time - m_EnterTime;
               
        if (deltaTime < m_HitStunTime)// hit stunning lasting
        {
            Knockback(m_StunningTime, deltaTime, m_KnockbackDistance, m_HitStunTime);
            m_StunningTime = deltaTime;
            return;
        }
        else if (HasHitStunning())// hit stunning ends
        {
            m_AIController.RemoveAdditiveState(ECharacterState.Hurt);
        }

        if (deltaTime >= TRANSITION_INTERVAL)
        {
            m_AIController.RemoveAdditiveState(ECharacterState.Hurt);
        }
    }    

    private float CalcHitTween(Vector3 hitPos)
    {
        Vector3 l = hitPos - m_AIController.capsule.bounds.center;
        bool isRight = Vector3.Cross(m_AIController.transform.forward, l.normalized).y > 0f;
        float dot = Vector3.Dot(l.normalized, m_AIController.transform.right);
        float ratio = Mathf.Abs(dot) * l.magnitude / m_AIController.capsule.bounds.extents.x;
        ratio = ratio * (isRight ? -1f : 1f);
        //Debug.Log($"hit pos[{hitPos}], center[{m_AIController.capsule.bounds.center}], is right[{isRight}], dot[{dot}], ratio[{ratio}]");        
        return ratio;
    }

    private bool HasHitStunning()
    {
        return m_HitStunTime > 0f;
    }

    private void Knockback(float lastStunningTime, float stunningTime, float distance, float duration)
    {
        if (Mathf.Approximately(distance, 0f) || Mathf.Approximately(duration, 0f))
            return;

        if(m_Source == null)
            return;
        
        float deltaDistance = (stunningTime - lastStunningTime) / duration * distance;

        Vector3 knockback = (m_AIController.modelTransform.position - m_Source.modelTransform.position).normalized * deltaDistance;
        knockback.y = 0f;
        m_AIController.transform.Translate(knockback, Space.World);
    }
}
