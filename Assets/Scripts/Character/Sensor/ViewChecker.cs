using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ViewChecker
{
    public float fieldOfView = 135f;
    public float eyeHeightOffset = 1f;
    public float sightDistance = 7f;
    public Color gizmosColor = Color.blue;    

    //TODO: use serializable dictionary
    public string targetTag;    

    private Transform m_Host;

    public Transform host
    {
        get => m_Host;
        set => m_Host = value;
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
        if(m_Host == null) return false;

        float dot = Vector3.Dot(direction, m_Host.forward);
        if (dot < 0f) return false;

        float angle = Vector3.Angle(m_Host.forward, direction);
        return angle < fieldOfView / 2f;
    }

    /// <summary>
    /// Returns all visible AI targets sorted by distance (nearest first).
    /// Used by LockTargetManager for target switching.
    /// </summary>
    public List<Transform> FindVisibleTargets()
    {
        var results = new List<Transform>();
        if (m_Host == null) return results;

        using (var iter = AIManager.instance.enumerator)
        {
            while (iter.MoveNext())
            {
                var character = iter.Current.Value;

                if (character == null) continue;

                if (CanSeeObject(character.transform))
                    results.Add(character.transform);
            }
        }

        // Sort by distance (nearest first)
        results.Sort((a, b) =>
        {
            float distA = Vector3.Distance(m_Host.position, a.position);
            float distB = Vector3.Distance(m_Host.position, b.position);
            return distA.CompareTo(distB);
        });

        return results;
    }

    /// <summary>
    /// Finds the best target within a cone in front of the host.
    /// Used by LockTargetManager for initial hard-lock acquisition.
    /// </summary>
    /// <param name="forward">Cone direction (typically camera forward, y=0 normalized)</param>
    /// <param name="halfAngleDeg">Half-angle of the cone in degrees</param>
    /// <param name="maxDistance">Maximum distance to consider</param>
    /// <returns>The best target or null</returns>
    public Transform FindBestTargetInCone(Vector3 forward, float halfAngleDeg, float maxDistance)
    {
        if (m_Host == null) return null;

        Transform bestTarget = null;
        float bestScore = float.MaxValue;

        using (var iter = AIManager.instance.enumerator)
        {
            while (iter.MoveNext())
            {
                var character = iter.Current.Value;
                if (character == null) continue;

                Vector3 toTarget = character.transform.position - m_Host.position;
                toTarget.y = 0;
                float distance = toTarget.magnitude;
                if (distance > maxDistance || distance < Mathf.Epsilon) continue;

                float angle = Vector3.Angle(forward, toTarget.normalized);
                if (angle > halfAngleDeg) continue;

                if (!CanSeeObject(character.transform)) continue;

                // Score: prefer closer targets, slightly prefer targets closer to center
                float score = distance + angle * 0.1f;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = character.transform;
                }
            }
        }
        return bestTarget;
    }

    public void DrawViewRange()
    {
        if (m_Host == null || m_Host.gameObject == null) return;

        Vector3 eyePosition = m_Host.position + m_Host.up * eyeHeightOffset;
        Vector3 dir1 = Quaternion.AngleAxis(fieldOfView / 2, m_Host.up) * m_Host.forward;
        Vector3 dir2 = Quaternion.AngleAxis(-fieldOfView / 2, m_Host.up) * m_Host.forward;
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
            Vector3 d = Quaternion.AngleAxis(currentAngle, m_Host.up) * m_Host.forward;
            d.Normalize();
            currentAnglePt = eyePosition + d * sightDistance;
            Debug.DrawLine(lastPt, currentAnglePt, gizmosColor);
            lastPt = currentAnglePt;
        }
    }
}
