using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

//TODO: Support multiple objects
public class CanSeeObject : Conditional
{
    public float fieldOfView = 135f;
    public float eyeHeightOffset = 1f;
    public float sightDistance = 7f;

    public string targetTag;
    public Color gizmosColor = Color.red;

    public SharedTransform target;

    private ViewChecker m_ViewChecker;

    public override void OnAwake()
    {
        m_ViewChecker = new ViewChecker(this.transform);
        m_ViewChecker.fieldOfView = fieldOfView;
        m_ViewChecker.eyeHeightOffset = eyeHeightOffset;
        m_ViewChecker.sightDistance = sightDistance;
        m_ViewChecker.targetTag = targetTag;
        m_ViewChecker.gizmosColor = gizmosColor;
    }

    public override TaskStatus OnUpdate()
    {
        if(target == null || target.Value == null) return TaskStatus.Failure;

        m_ViewChecker.forward = m_ViewChecker.forward.NormalizeIgnoreY();

        if (m_ViewChecker.CanSeeObject(target.Value)) return TaskStatus.Success;

        return TaskStatus.Running;
    }

    public override void OnDrawGizmos()
    {
        m_ViewChecker?.DrawViewRange();
    }
}
