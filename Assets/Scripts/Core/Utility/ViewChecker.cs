using UnityEngine;

public class ViewChecker : MonoBehaviour
{
    public float fieldOfView = 90f;
    public float eyeHeightOffset = 1f;
    public float sightDistance = 7f;
    public Color gizmosColor = Color.blue;

    //TODO: use serializable dictionary
    public string targetTag;

    public bool CanSeeObject(Transform target)
    { 
        if (target == null) return false;

        if (!string.IsNullOrEmpty(targetTag) && !target.gameObject.CompareTag(targetTag)) return false;

        Vector3 eyePosition = transform.position + transform.up * eyeHeightOffset;
        float dist = Vector3.Distance(eyePosition, target.position);

        if (dist > sightDistance) return false;

        Vector3 dir = target.position - transform.position;
        dir.Normalize();

        float dot = Vector3.Dot(dir, transform.forward);
        if (dot < 0f) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        return angle < fieldOfView / 2f;
    }

    public void DrawViewRange()
    {
        if (transform == null || transform.gameObject == null) return;

        Vector3 eyePosition = transform.position + transform.up * eyeHeightOffset;
        Vector3 dir1 = Quaternion.AngleAxis(fieldOfView / 2, transform.up) * transform.forward;
        Vector3 dir2 = Quaternion.AngleAxis(-fieldOfView / 2, transform.up) * transform.forward;
        dir1.Normalize();
        dir2.Normalize();
        Vector3 startPt = eyePosition + dir1 * sightDistance;
        Vector3 endPt = eyePosition + dir2 * sightDistance;

        Debug.DrawLine(eyePosition, startPt, gizmosColor);
        Debug.DrawLine(eyePosition, endPt, gizmosColor);

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
            Debug.DrawLine(lastPt, currentAnglePt, gizmosColor);
            lastPt = currentAnglePt;
        }
    }

    private void OnDrawGizmos()
    {
        DrawViewRange();
    }
}
