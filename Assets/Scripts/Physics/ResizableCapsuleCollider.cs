using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class ResizableCapsuleCollider : MonoBehaviour
{    
    public CapsuleColliderData colliderData { get { return m_CapsuleColliderData; } }
    public SlopeData slopeData { get { return m_SlopeData; } }

    /// <summary>
    /// Position of center in world space.
    /// </summary>
    public Vector3 center
    {
        get { return m_CapsuleColliderData.collider.bounds.center; }
    }

    /// <summary>
    /// Walkable check box's center position in world space
    /// </summary>
    public Vector3 checkBoxCenter
    {
        get { return m_WalkableCheckBox.bounds.center; }
    }

    /// <summary>
    /// Walkable check box's extent
    /// </summary>
    public Vector3 checkBoxExtents
    {
        get { return m_WalkableCheckBox.bounds.extents; }
    }

    [SerializeField] private CapsuleColliderData m_CapsuleColliderData = null;
    [SerializeField] private DefaultColliderData m_DefaultColliderData;
    [SerializeField] private SlopeData m_SlopeData;
    private BoxCollider m_WalkableCheckBox = null;
    private Queue<float> m_StepHeightPercentQueue;

    private void Awake()
    {
        Resize();
    }

    private void Start()
    {
        FindWalkableCheckBox();
    }

    private void OnValidate()
    {
        Resize();
    }

    public void Resize()
    {
        Initialize(transform.gameObject);

        CalculateCapsuleColliderDimensions();
    }

    public void SetStepHeightPercent(float val)
    {
        m_SlopeData.stepHeightPercentage = Mathf.Clamp01(val);
        Resize();
    }

    public void StoreStepHeightPercent()
    { 
        m_StepHeightPercentQueue.Enqueue(m_SlopeData.stepHeightPercentage);        
    }

    public void RestoreStepHeightPercent()
    {
        if (m_StepHeightPercentQueue.Count == 0)
            return;

        m_SlopeData.stepHeightPercentage = m_StepHeightPercentQueue.Dequeue();
        Resize();
    }

    private void Initialize(GameObject gameObject)
    {
        if ((m_CapsuleColliderData != null && m_CapsuleColliderData.collider != null) || gameObject == null)
        {
            return;
        }

        m_StepHeightPercentQueue = new Queue<float>();        

        m_SlopeData = new SlopeData(0.25f, 2f, 25f);

        if (m_CapsuleColliderData == null)
            m_CapsuleColliderData = new CapsuleColliderData();

        m_CapsuleColliderData.Initialize(gameObject);

        if (m_CapsuleColliderData.collider != null)
        {
            m_DefaultColliderData = new DefaultColliderData(m_CapsuleColliderData.collider.height,
                m_CapsuleColliderData.collider.center.y,
                m_CapsuleColliderData.collider.radius);
        }
    }

    private void CalculateCapsuleColliderDimensions()
    {
        SetCapsuleColliderRadius(m_DefaultColliderData.radius);

        SetCapsuleColliderHeight(m_DefaultColliderData.height * (1f - m_SlopeData.stepHeightPercentage));

        RecalculateCapsuleColliderCenter();

        RecalculateColliderRadius();

        m_CapsuleColliderData.UpdateColliderData();
    }

    private void SetCapsuleColliderRadius(float radius)
    {
        m_CapsuleColliderData.collider.radius = radius;
    }

    private void SetCapsuleColliderHeight(float height)
    {
        m_CapsuleColliderData.collider.height = height;
    }

    private void RecalculateCapsuleColliderCenter()
    {
        float colliderHeightDifference = m_DefaultColliderData.height - m_CapsuleColliderData.collider.height;

        Vector3 newColliderCenter = new Vector3(0f, m_DefaultColliderData.centerY + (colliderHeightDifference / 2f), 0f);

        m_CapsuleColliderData.collider.center = newColliderCenter;
    }

    private void RecalculateColliderRadius()
    {
        float halfColliderHeight = m_CapsuleColliderData.collider.height / 2f;

        if (halfColliderHeight >= m_CapsuleColliderData.collider.radius)
        {
            return;
        }

        SetCapsuleColliderRadius(halfColliderHeight);
    }

    private void FindWalkableCheckBox()
    {
        try
        {
            m_WalkableCheckBox = transform.Find("CollideCheck").Find("WalkableCheck").GetComponent<BoxCollider>();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}
