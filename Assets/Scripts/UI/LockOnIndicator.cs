using UnityEngine;

/// <summary>
/// Visual animation for a single hard-lock indicator: rotation + breathing scale.
/// Attached to the indicator Image's RectTransform.
/// </summary>
public class LockOnIndicator : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float m_RotateSpeed = 180f;        // Rotation speed (deg/s)
    [SerializeField] private float m_BreatheAmplitude = 0.08f;  // Breathing scale amplitude (0~1)
    [SerializeField] private float m_BreatheFrequency = 2.5f;   // Breathing frequency (Hz)

    private RectTransform m_Rect;
    private float m_BaseScale = 1f;

    private void Awake()
    {
        m_Rect = GetComponent<RectTransform>();
        if (m_Rect != null)
            m_BaseScale = m_Rect.localScale.x;
    }

    private void Update()
    {
        if (m_Rect == null)
            return;

        // Rotation
        m_Rect.Rotate(0f, 0f, m_RotateSpeed * Time.deltaTime);

        // Breathing scale
        float breathe = 1f + m_BreatheAmplitude * Mathf.Sin(Time.time * m_BreatheFrequency);
        m_Rect.localScale = Vector3.one * (m_BaseScale * breathe);
    }
}

