using System;
using UnityEngine;

public delegate void DistanceZoneChangeDelegate(EDistanceZone newZone, EDistanceZone oldZone, float distance);

[Serializable]
public class DistanceZone
{
    [SerializeField] private DistanceZoneSettings m_Settings;

    private EDistanceZone m_Zone = EDistanceZone.Far;    
    private float m_Distance = 0.0f;

    private Transform m_Target = null;
    private Transform m_Source = null;

    private DistanceZoneChangeDelegate m_ZoneChangeNotify;

    public EDistanceZone zone => m_Zone;
    public float distance => m_Distance;
    public Transform target
    {
        get => m_Target;
        set => m_Target = value;
    }
    public Transform source
    {
        get => m_Source;
        set => m_Source = value;
    }

    public void Update()
    {
        if (m_Settings == null) throw new Exception("DistanceZoneSettings hasn't been configured!");        
        if(m_Target == null || m_Source == null) return;

        m_Distance = Vector3.Distance(m_Source.position, m_Target.position);
        var currentZone = m_Settings.GetZone(m_Distance);
        if (m_Zone != currentZone)
        {
            m_ZoneChangeNotify?.Invoke(currentZone, m_Zone, m_Distance);
            m_Zone = currentZone;
        }      
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
