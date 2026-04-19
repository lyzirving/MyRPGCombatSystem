using System.Collections.Generic;
using UnityEngine;

public interface ICharacterScanListener
{
    void OnTargetLost(CharacterControllerBase target);
    void OnTargetFound(CharacterControllerBase target);
    void OnTargetChange(CharacterControllerBase target, CharacterControllerBase last);
}

public class CharacterScan : MonoBehaviour
{
    public float scanInterval = 1f;

    private float m_LastScanTime;
    private CharacterControllerBase m_Target;
    private float m_TargetDistance;

    private ViewChecker m_ViewChecker;
    private List<ICharacterScanListener> m_Listeners = new List<ICharacterScanListener>();

    private void Awake()
    {
        m_LastScanTime = Time.time;
        ResetTarget();
    }

    private void Start()
    {
        m_ViewChecker = GetComponent<ViewChecker>();
    }

    private void Update()
    {
        if (Time.time - m_LastScanTime > scanInterval)
        {
            Scan();
            m_LastScanTime = Time.time;
        }
    }

    private void OnDestroy()
    {
        m_Listeners.Clear();
    }

    public bool IsDirectionInView(Vector3 direction)
    { 
        return m_ViewChecker.IsDirectionInView(direction);
    }

    public void AddListener(ICharacterScanListener listener)
    {
        foreach (var item in m_Listeners)
        {
            if (item == listener)
                return;
        }

        m_Listeners.Add(listener);
    }

    public void RemoveListener(ICharacterScanListener listener)
    {
        for (int i = 0; i < m_Listeners.Count; ++i)
        {
            if (m_Listeners[i] == listener)
            { 
                m_Listeners.RemoveAt(i);
                return;
            }
        }
    }

    private void Scan()
    {
        if (m_ViewChecker == null)
        {
            Debug.LogWarning($"go[{gameObject.name}] doesn't have ViewChecker");
            return;
        }

        CharacterControllerBase lastTarget = m_Target;
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
                float dist = Vector3.Distance(this.transform.position, character.transform.position);
                if(dist < m_TargetDistance && m_ViewChecker.CanSeeObject(character.transform))
                {
                    m_TargetDistance = dist;
                    m_Target = character;
                }
            }
        }

        NotifyTargetChange(m_Target, lastTarget);
    }

    private void ResetTarget()
    {
        m_Target = null;
        m_TargetDistance = float.MaxValue;
    }

    private void NotifyTargetChange(CharacterControllerBase current, CharacterControllerBase last)
    {
        if (current == last) return;

        if (current != null && last == null)
        {
            foreach (var item in m_Listeners)
                item.OnTargetFound(current);
        }
        else if (current == null && last != null)
        {
            foreach (var item in m_Listeners)
                item.OnTargetLost(last);
        }
        else
        {
            foreach (var item in m_Listeners)
                item.OnTargetChange(current, last);
        }
    }
}
