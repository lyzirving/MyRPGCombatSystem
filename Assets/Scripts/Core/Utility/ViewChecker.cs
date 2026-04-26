using System;
using UnityEngine;

[Serializable]
public class ViewChecker
{
    public float fieldOfView = 90f;
    public float eyeHeightOffset = 1f;
    public float sightDistance = 7f;
    public Color gizmosColor = Color.blue;    

    //TODO: use serializable dictionary
    public string targetTag;

    public Vector3 forward
    {
        get => m_Forward; 
        set => m_Forward = value;
    }

    private Transform m_Host;
    private Vector3 m_Forward;

    public ViewChecker(Transform host)
    {
        m_Host = host;
        m_Forward = host.forward;
    }

    public bool CanSeeObject(Transform target)
    { 
        if (target == null) return false;

        if (!string.IsNullOrEmpty(targetTag) && !target.gameObject.CompareTag(targetTag)) return false;

        Vector3 eyePosition = m_Host.position + m_Host.up * eyeHeightOffset;
        float dist = Vector3.Distance(eyePosition, target.position);

        if (dist > sightDistance) return false;

        Vector3 dir = target.position - m_Host.position;
        dir.Normalize();

        return IsDirectionInView(dir);
    }

    public bool IsDirectionInView(Vector3 direction)
    {
        float dot = Vector3.Dot(direction, m_Forward);
        if (dot < 0f) return false;

        float angle = Vector3.Angle(m_Forward, direction);
        return angle < fieldOfView / 2f;
    }

    public void DrawViewRange()
    {
        if (m_Host == null || m_Host.gameObject == null) return;

        Vector3 eyePosition = m_Host.position + m_Host.up * eyeHeightOffset;
        Vector3 dir1 = Quaternion.AngleAxis(fieldOfView / 2, m_Host.up) * m_Forward;
        Vector3 dir2 = Quaternion.AngleAxis(-fieldOfView / 2, m_Host.up) * m_Forward;
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
            Vector3 d = Quaternion.AngleAxis(currentAngle, m_Host.up) * m_Forward;
            d.Normalize();
            currentAnglePt = eyePosition + d * sightDistance;
            Debug.DrawLine(lastPt, currentAnglePt, gizmosColor);
            lastPt = currentAnglePt;
        }
    }
}
