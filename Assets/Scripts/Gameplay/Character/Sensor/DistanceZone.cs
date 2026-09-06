using System;
using UnityEngine;

[Serializable]
public class DistanceZone
{
    public delegate void DistanceChangeNotify(EDistanceZone newZone, EDistanceZone oldZone, float distance);

    [SerializeField] private DistanceZoneSettings m_Settings;

    private EDistanceZone m_Zone = EDistanceZone.Far;
    private float m_Distance = 0.0f;

    private Transform m_Host = null;
    private Transform m_Target = null;
    private DistanceChangeNotify m_ZoneChangeNotify;

    public float distance => m_Distance;

    public DistanceChangeNotify onChange
    {
        get => m_ZoneChangeNotify;
        set => m_ZoneChangeNotify = value;
    }
    
    public Transform target
    {
        get => m_Target;
        set
        {
            m_Target = value;
            UpdateDistance();
        }
    }

    public Transform host
    {
        get => m_Host;
        set
        {
            m_Host = value;
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

    public bool IsZone(EDistanceZone zone) => m_Zone == zone;

    public void UpdateDistance()
    {
        if (m_Settings == null) throw new Exception("DistanceZoneSettings hasn't been configured!");

        if (m_Target == null || m_Host == null)
        {
            zone = EDistanceZone.None;
            return;
        }

        Vector3 currentPos = m_Host.position;
        Vector3 targetPos = m_Target.position;
        currentPos.y = 0f;
        targetPos.y = 0f;

        m_Distance = Vector3.Distance(currentPos, targetPos);
        var currentZone = m_Settings.GetZone(m_Distance);
        zone = currentZone;
    }
}
