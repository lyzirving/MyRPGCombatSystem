using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

//TODO: Support multiple objects
public class CanSeeObject : Conditional
{
    public float fieldOfView = 90f;
    public float eyeHeightOffset = 1f;
    public float sightDistance = 7f;

    public string targetTag;
    public Color gizmosColor = Color.red;

    public SharedTransform target;

    private ViewChecker m_ViewChecker;

    public override void OnAwake()
    {
        m_ViewChecker = GetComponent<ViewChecker>();
        if (m_ViewChecker == null)
        {
            m_ViewChecker = this.gameObject.AddComponent<ViewChecker>();           
        }
        m_ViewChecker.fieldOfView = fieldOfView;
        m_ViewChecker.eyeHeightOffset = eyeHeightOffset;
        m_ViewChecker.sightDistance = sightDistance;
        m_ViewChecker.targetTag = targetTag;
        m_ViewChecker.gizmosColor = gizmosColor;
    }

    public override TaskStatus OnUpdate()
    {
        if(target == null || target.Value == null) return TaskStatus.Failure;

        if (m_ViewChecker.CanSeeObject(target.Value))
        {
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }

    public override void OnDrawGizmos()
    {
        if(m_ViewChecker == null) return;

        m_ViewChecker.DrawViewRange();
    }
}
