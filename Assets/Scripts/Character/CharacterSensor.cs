using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class CharacterSensor : MonoBehaviour 
{
    public bool isGrounded = false;

    private CapsuleCollider m_CapsuleCollider;

    private void Awake()
    {
        m_CapsuleCollider = GetComponent<CapsuleCollider>();        
    }    

    /// <summary>
    /// Check whether the character touches the ground
    /// </summary>
    /// <param name="characterTransform"></param>
    /// <param name="radius"></param>
    /// <param name="layerMask"></param>
    /// <param name="raycastHit"></param>
    /// <param name="skinWidth"></param>
    /// <param name="groundCheckOffset"></param>
    /// <returns></returns>
    public bool SphereCheckGround(Transform characterTransform, float radius, LayerMask layerMask, out RaycastHit raycastHit, float skinWidth = 0f, float groundCheckOffset = 0f)
    {
        return Physics.SphereCast(characterTransform.position + Vector3.up * groundCheckOffset,
            radius, Vector3.down, out raycastHit, 
            Mathf.Abs(groundCheckOffset - radius) + 2f * skinWidth, layerMask);
    }

    /// <summary>
    /// SphereCheckGround using Character's attribute
    /// </summary>
    /// <param name="layerMask"></param>
    /// <param name="raycastHit"></param>
    /// <param name="skinWidth"></param>
    /// <param name="groundCheckOffset"></param>
    /// <returns></returns>
    public bool SphereCheckGround(LayerMask layerMask, out RaycastHit raycastHit, float skinWidth = 0.1f, float groundCheckOffset = 0.5f)
    {
        return SphereCheckGround(this.transform, m_CapsuleCollider.radius, layerMask, out raycastHit, skinWidth, groundCheckOffset);
    }
}
