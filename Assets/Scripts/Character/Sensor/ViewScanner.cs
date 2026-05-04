using System;
using UnityEngine;

[Serializable]
public class ViewScanner
{
    public delegate void TargetFind(Transform target);
    public delegate void TargetLost(Transform target);
    public delegate void TargetChange(Transform current, Transform last);

    public float interval = 0.05f;
    [SerializeField] private ViewChecker m_ViewChecker = new ViewChecker();

    private float m_LastScanTime;
    private float m_TargetDistance;

    private Transform m_Host;
    private Transform m_Target;

    private TargetFind m_OnTargetFind;
    private TargetLost m_OnTargetLost;
    private TargetChange m_OnTargetChange;

    public TargetFind onFind
    {
        get => m_OnTargetFind;
        set => m_OnTargetFind = value;
    }

    public TargetLost onLost
    {
        get => m_OnTargetLost;
        set => m_OnTargetLost = value;
    }

    public TargetChange onChange
    {
        get => m_OnTargetChange;
        set => m_OnTargetChange = value;
    }

    #region Main Methods
    public void Init(Transform host)
    {
        m_Host = host;
        m_ViewChecker.host = host;
        m_LastScanTime = Time.time;

        ResetTarget();
    }

    public void DrawGizmos()
    {
        m_ViewChecker?.DrawViewRange();
    }
    #endregion

    #region Sensor Methods
    public void Scan()
    {
        m_ViewChecker.forward = m_Host.forward.NormalizeIgnoreY();
        // TODO: Place Scan() in a worker thread, only sync result and character attribute change.
        DoScan();       
    }

    public bool CanSeeObject(Transform transform)
    { 
        return m_ViewChecker.CanSeeObject(transform);
    }

    public bool WithinView(Vector3 direction)
    {
        return m_ViewChecker.IsDirectionInView(direction);
    }
    #endregion

    #region Private Methods
    private void DoScan()
    {
        if(m_Host == null) return;

        if (!m_Host.gameObject.tag.Equals("Player")) return;

        if (Time.time - m_LastScanTime < interval) return;

        m_LastScanTime = Time.time;

        Transform lastTarget = m_Target;
        ResetTarget();

        if (AIManager.instance.isEmpty)
        {
            NotifyTargetChange(null, lastTarget);
            return;
        }

        using (var iter = AIManager.instance.enumerator)
        {
            while (iter.MoveNext())
            {
                var character = iter.Current.Value;
                float dist = Vector3.Distance(m_Host.position, character.transform.position);
                if (dist < m_TargetDistance && m_ViewChecker.CanSeeObject(character.transform))
                {
                    m_TargetDistance = dist;
                    m_Target = character.transform;
                }
            }
        }

        NotifyTargetChange(m_Target, lastTarget);
    }

    private void NotifyTargetChange(Transform current, Transform last)
    {
        if (current == last) return;

        if (current != null && last == null)
        {
            m_OnTargetFind?.Invoke(current);
        }
        else if (current == null && last != null)
        {
            m_OnTargetLost?.Invoke(last);
        }
        else
        {
            m_OnTargetChange?.Invoke(current, last);
        }
    }

    private void ResetTarget()
    {
        m_Target = null;
        m_TargetDistance = float.MaxValue;
    }
    #endregion
}
