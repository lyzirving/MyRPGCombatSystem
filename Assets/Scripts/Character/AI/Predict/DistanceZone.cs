using System;
using UnityEngine;

public delegate void DistanceZoneChangeDelegate(EDistanceZone newZone, EDistanceZone oldZone, float distance);

[Serializable]
public class DistanceZone : MonoBehaviour
{
    [SerializeField] private DistanceZoneSettings m_Settings;

    private EDistanceZone m_Zone = EDistanceZone.Far;    
    private float m_Distance = 0.0f;

    private Transform m_Target = null;

    private DistanceZoneChangeDelegate m_ZoneChangeNotify;
    
    public float distance => m_Distance;
    public Transform target
    {
        get => m_Target;
        set
        {
            m_Target = value;
            UpdateDistance();
        }
    }

    public EDistanceZone zone
    {
        get => m_Zone;
        set
        {
            EDistanceZone last = m_Zone;
            m_Zone = value;
            if (m_Zone != last)
                m_ZoneChangeNotify?.Invoke(m_Zone, last, m_Distance);
        }
    }

    private void Update()
    {
        UpdateDistance();
    }

    public void UpdateDistance()
    {
        if (m_Settings == null) throw new Exception("DistanceZoneSettings hasn't been configured!");

        if (m_Target == null)
        {
            zone = EDistanceZone.None;
            return;
        }
        Vector3 currentPos = transform.position;
        Vector3 targetPos = m_Target.position;
        currentPos.y = 0f;
        targetPos.y = 0f;

        m_Distance = Vector3.Distance(currentPos, targetPos);
        var currentZone = m_Settings.GetZone(m_Distance);
        zone = currentZone;   
    }

    public void AddZoneChangeNotify(DistanceZoneChangeDelegate method)
    {
        m_ZoneChangeNotify += method;
    }

    public void RemoveZoneChangeNotify(DistanceZoneChangeDelegate method)
    {
        m_ZoneChangeNotify -= method;
    }
}
