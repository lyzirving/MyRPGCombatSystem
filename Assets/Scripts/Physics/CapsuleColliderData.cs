using System;
using UnityEngine;

[Serializable]
public struct DefaultColliderData
{
    [Tooltip("The height is known through the Model Mesh Renderer \"bounds.size\" variable.")]
    [SerializeField]
    public float height;

    [SerializeField]
    public float centerY;

    [SerializeField]
    public float radius;

    public DefaultColliderData(float inHeight, float inCenterY, float inRadius)
    { 
        height = inHeight;
        centerY = inCenterY;
        radius = inRadius;
    }
}

[Serializable]
public struct SlopeData
{
    [SerializeField]
    [Range(0f, 1f)]
    public float stepHeightPercentage;

    [SerializeField]
    [Range(0f, 5f)]
    public float floatRayDistance;

    [SerializeField]
    [Range(0f, 50f)]
    public float stepReachForce;

    public SlopeData(float inStepHeightPercentage, float inFloatRayDistance, float inStepReachForce)
    {
        stepHeightPercentage = inStepHeightPercentage;
        floatRayDistance = inFloatRayDistance;
        stepReachForce = inStepReachForce;
    }
}

[Serializable]
public class CapsuleColliderData
{
    public CapsuleCollider collider;

    [SerializeField]
    public Vector3 centerInLocalSpace;

    [SerializeField]
    // Always the half size of the bounds
    public Vector3 verticalExtents;

    public void Initialize(GameObject gameObject)
    {
        if (collider != null || gameObject == null)
            return;

        collider = gameObject.GetComponent<CapsuleCollider>();

        UpdateColliderData();
    }

    public void UpdateColliderData()
    {
        centerInLocalSpace = collider.center;

        verticalExtents = new Vector3(0f, collider.bounds.extents.y, 0f);
    }
}
