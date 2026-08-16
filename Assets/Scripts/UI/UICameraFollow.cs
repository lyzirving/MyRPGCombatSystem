using UnityEngine;

/// <summary>
/// Keeps a UI overlay camera perfectly aligned with the main camera.
///
/// The UI camera renders the UI layer (World Space Canvas) on top of the
/// main scene output. It must share the main camera's position, rotation,
/// and projection so billboarded UI elements align with the world.
///
/// Sync happens in LateUpdate after the main camera (and any Cinemachine
/// brain) has moved for the frame.
/// </summary>
public class UICameraFollow : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Main camera to follow. Leave null to auto-find via Camera.main.")]
    [SerializeField] private Camera m_MainCamera;

    [Header("Sync Options")]
    [Tooltip("Sync position each frame.")]
    [SerializeField] private bool m_SyncPosition = true;

    [Tooltip("Sync rotation each frame.")]
    [SerializeField] private bool m_SyncRotation = true;

    [Tooltip("Sync projection parameters (FOV / orthographic size) each frame.")]
    [SerializeField] private bool m_SyncProjection = true;

    [Tooltip("Sync near/far clip planes each frame.")]
    [SerializeField] private bool m_SyncClipPlanes = false;

    private Camera m_UICamera;

    private void Awake()
    {
        m_UICamera = GetComponent<Camera>();
        if (m_MainCamera == null)
            m_MainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (m_MainCamera == null || m_UICamera == null)
            return;

        var t = transform;
        var mainT = m_MainCamera.transform;

        if (m_SyncPosition)
            t.position = mainT.position;

        if (m_SyncRotation)
            t.rotation = mainT.rotation;

        if (m_SyncProjection)
        {
            m_UICamera.orthographic = m_MainCamera.orthographic;
            if (m_MainCamera.orthographic)
                m_UICamera.orthographicSize = m_MainCamera.orthographicSize;
            else
                m_UICamera.fieldOfView = m_MainCamera.fieldOfView;
        }

        if (m_SyncClipPlanes)
        {
            m_UICamera.nearClipPlane = m_MainCamera.nearClipPlane;
            m_UICamera.farClipPlane = m_MainCamera.farClipPlane;
        }
    }
}
