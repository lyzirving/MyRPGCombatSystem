using UnityEngine;

public class PlayerModel : CharacterModel
{
    [Header("Soft Lock Look-At (Head Tracking)")]
    [SerializeField] private float m_LookAtMaxAngle = 50f;

    [SerializeField] private float m_LookAtMaxWeight = 0.8f;
    
    [SerializeField] private float m_LookAtSmoothSpeed = 8f;

    [SerializeField] private float m_LookAtTargetHeight = 1.5f;

    private CharacterControllerBase m_Controller;

    private Transform m_SoftTarget;
    private Vector3 m_CurrentLookAtPos;
    private float m_CurrentLookWeight;

    public override void Init(ICharacterBehavior characterBehavior)
    {
        base.Init(characterBehavior);
        m_Controller = characterBehavior as CharacterControllerBase;
    }

    private void LateUpdate()
    {
        m_SoftTarget = m_Controller?.softLockTarget;
    }

    /// <summary>
    /// Animator IK Callback（should select IK Pass in animator layer）。
    /// </summary>
    private void OnAnimatorIK(int layerIndex)
    {
        LookAtTargetIK(layerIndex);
    }

    private void LookAtTargetIK(int layerIndex)
    {
        if (layerIndex != AnimationConsts.BASE_LAYER)
            return;

        float targetWeight = 0f;

        if (m_SoftTarget != null)
        {
            Vector3 toTarget = m_SoftTarget.position - transform.position;
            toTarget.y = 0;

            if (toTarget.sqrMagnitude > 0.01f)
            {
                float angle = Vector3.Angle(transform.forward, toTarget.normalized);

                if (angle < m_LookAtMaxAngle)
                {
                    targetWeight = m_LookAtMaxWeight * (1f - angle / m_LookAtMaxAngle);

                    m_CurrentLookAtPos = m_SoftTarget.position + Vector3.up * m_LookAtTargetHeight;
                }
            }
        }
                
        // smooth transition, avoid sudden change
        m_CurrentLookWeight = Mathf.Lerp(
            m_CurrentLookWeight, targetWeight, Time.deltaTime * m_LookAtSmoothSpeed);

        // apply ik weight
        // params: totalWeight, bodyWeight, headWeight, eyeWeight, clampWeight
        m_Animator.SetLookAtWeight(m_CurrentLookWeight, 0.3f, 1f, 0f, 0.5f);

        if (m_CurrentLookWeight > 0.01f)
            m_Animator.SetLookAtPosition(m_CurrentLookAtPos);
    }
}
