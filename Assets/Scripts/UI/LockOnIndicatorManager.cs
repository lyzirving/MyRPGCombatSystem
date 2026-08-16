using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hard-lock target indicator manager (World Space billboard).
///
/// Places the indicator at the locked target's world position + height offset,
/// and orients it to always face the camera (billboard).
///
/// Behavior:
///   - Target locked → indicator follows the target, facing the camera
///   - No locked target → indicator hides
/// </summary>
[RequireComponent(typeof(Canvas))]
public class LockOnIndicatorManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player root (has LockTargetManager attached)")]
    [SerializeField] private Transform m_Player;

    [Tooltip("Indicator prefab (RectTransform + Image + LockOnIndicator)")]
    [SerializeField] private RectTransform m_IndicatorPrefab;

    [Header("Config")]
    [Tooltip("Height offset above the target's head (meters)")]
    [SerializeField] private float m_HeightOffset = 2f;

    [Tooltip("World-space scale of the indicator canvas. 1px = this many meters.")]
    [SerializeField] private float m_WorldScale = 0.005f;

    [Header("Rendering")]
    [Tooltip("UI camera that renders this World Space canvas on top of the main scene (overlay). Leave null to auto-find by tag 'UICamera'.")]
    [SerializeField] private Camera m_UICamera;
    [SerializeField] private Camera m_MainCamera;

    private Camera m_Cam;
    private LockTargetManager m_LockManager;
    private Canvas m_IndicatorCanvas;
    private GameObject m_Indicator;

    #region Unity Lifecycle

    private void Awake()
    {
        m_IndicatorCanvas = GetComponent<Canvas>();
        m_IndicatorCanvas.renderMode = RenderMode.WorldSpace;
        m_IndicatorCanvas.transform.localScale = Vector3.one * m_WorldScale;

        // Assign the canvas to the UI layer so only the UI camera renders it.
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer >= 0)
            gameObject.layer = uiLayer;

        m_LockManager = m_Player != null ? m_Player.GetComponent<LockTargetManager>() : null;

        if (m_LockManager == null)
        {
            Debug.LogWarning($"[LockOnIndicatorManager] LockTargetManager not found on player[{m_Player?.name}]", this);
        }

        // Resolve the UI camera (manual override first, then auto-find by tag)
        if (m_UICamera == null)
        {
            var uiCamGO = GameObject.FindGameObjectWithTag("UICamera");
            m_UICamera = uiCamGO != null ? uiCamGO.GetComponent<Camera>() : null;
        }

        // Spawn the indicator inside the canvas
        if (m_IndicatorPrefab != null)
        {
            m_Indicator = Instantiate(m_IndicatorPrefab, transform).gameObject;
            m_Indicator.layer = uiLayer;
            m_Indicator.SetActive(false);
        }
        else
        {
            CreateFallbackIndicator(transform);
        }
    }

    private void LateUpdate()
    {
        if (m_LockManager == null || m_UICamera == null || m_IndicatorCanvas == null || m_MainCamera == null)
            return;

        Transform target = m_LockManager.LockedTarget;

        if (target != null)
        {
            if(!m_Indicator.activeSelf)
                m_Indicator.SetActive(true);

            // Position: target world position + height offset
            m_Indicator.transform.position = target.position + Vector3.up * m_HeightOffset;
            m_Indicator.transform.rotation =
                Quaternion.LookRotation(m_MainCamera.transform.forward, Vector3.up);
        }
        else
        {
            // No locked target → hide
            if (m_Indicator.gameObject.activeSelf)
                m_Indicator.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Fallback

    /// <summary>
    /// Minimal debug indicator created when no prefab is assigned.
    /// </summary>
    private void CreateFallbackIndicator(Transform parent)
    {
        var go = new GameObject("LockOnIndicator_Debug", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        go.layer = parent.gameObject.layer;

        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(64f, 64f);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);

        var img = go.GetComponent<Image>();
        img.color = new Color(1f, 0.4f, 0.2f, 0.9f); // Warm orange

        go.AddComponent<LockOnIndicator>();

        go.SetActive(false);

        m_Indicator = go;
    }

    #endregion
}
