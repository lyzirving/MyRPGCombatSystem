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
    public SharedTransform target;

    public override TaskStatus OnUpdate()
    {
        if(target == null || target.Value == null) return TaskStatus.Failure;
        if(!string.IsNullOrEmpty(targetTag) && !target.Value.gameObject.CompareTag(targetTag)) return TaskStatus.Failure;

        if (IsTargetVisible(target.Value))
        {
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }

    public override void OnDrawGizmos()
    {
        if(transform == null || transform.gameObject == null) return;

        Vector3 eyePosition = transform.position + transform.up * eyeHeightOffset;
        Vector3 dir1 = Quaternion.AngleAxis(fieldOfView / 2, transform.up) * transform.forward;
        Vector3 dir2 = Quaternion.AngleAxis(-fieldOfView / 2, transform.up) * transform.forward;
        dir1.Normalize();
        dir2.Normalize();
        Vector3 startPt = eyePosition + dir1 * sightDistance;
        Vector3 endPt = eyePosition + dir2 * sightDistance;

        Debug.DrawLine(eyePosition, startPt, Color.red);
        Debug.DrawLine(eyePosition, endPt, Color.red);

        int itr = 20;
        float interval = fieldOfView / itr;
        float currentAngle = fieldOfView / 2;
        Vector3 lastPt = startPt;
        Vector3 currentAnglePt = startPt;
        for (int i = 1; i < itr + 1; ++i)
        {       
            currentAngle -= interval;
            Vector3 d = Quaternion.AngleAxis(currentAngle, transform.up) * transform.forward;
            d.Normalize();
            currentAnglePt = eyePosition + d * sightDistance;
            Debug.DrawLine(lastPt, currentAnglePt, Color.red);
            lastPt = currentAnglePt;
        }
    }

    private bool IsTargetVisible(Transform targetTransform)
    { 
        if(targetTransform == null) return false;
        Vector3 eyePosition = transform.position + transform.up * eyeHeightOffset;

        float dist = Vector3.Distance(eyePosition, targetTransform.position);
        if(dist > sightDistance) return false; 

        Vector3 dir = targetTransform.position - transform.position;
        dir.Normalize();

        float dot = Vector3.Dot(dir, transform.forward);
        if(dot < 0f) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        return angle < fieldOfView / 2f;
    }
}
