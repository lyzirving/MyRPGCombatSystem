using UnityEngine;

public class AIStateRoar : AIStateGround
{
    public Transform target;

    public override void Enter(StateBase exitState, ChangeStateArgs args)
    {
        m_AIController.model.TriggerAnimation(AnimationConsts.roar);
    }

    public override void Exit(StateBase newState)
    {
        target = null;
    }

    public override void Update()
    {
        if (!m_AIController.model.GetTargetAnimationTime("Roar", AnimationConsts.BASE_LAYER, out float time))
        {
            Debug.LogError("Fail to get Roar animation's time");
            m_AIController.ChangeState(ECharacterState.Idle);
            return;
        }

        if (time >= 0.9f)
        {
            m_AIController.ChangeState(ECharacterState.Idle);
        }
    }

    public override void FixedUpdate()
    {
        if(target != null)
        {
            Vector3 targetDir = target.position - m_AIController.transform.position;
            targetDir.Normalize();
            m_AIController.RotateToTargetDir(targetDir, m_AIController.config.rotateSpeed);
        }
    }
}
