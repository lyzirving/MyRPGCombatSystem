using System.Collections.Generic;
using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    [Header("Attributes")]
    public SkinnedMeshRenderer meshRenderer;
    public List<Material> materials;

    public float lifeTime = 0f;
    public LayerMask meshLayerMask;

    [Header("Continous")]
    public float timeInterval = 0.1f;
    public float distInterval = 0.1f;

    private List<Ghost> m_Ghosts;
    private bool m_IsTrailing = false;
    private float m_LastCreateTime;
    private Vector3 m_LastCreatePosition;

    private void Awake()
    {
        m_Ghosts = new List<Ghost>();
        m_LastCreateTime = Time.time;
        m_LastCreatePosition = Vector3.positiveInfinity;
    }

    // Update is called once per frame
    private void Update()
    {
        BuildGhost();
        UpdateGhost();
    }

    public void BeginTrail()
    {
        m_IsTrailing = true;
    }

    public void EndTrail()
    {
        m_IsTrailing = false;
    }

    public void CreateSingleGhost()
    {
        if (meshRenderer == null)
        {
            Debug.LogWarning($"SkinnedMeshRenderer hasn't been assgined to GhostTrail[{this.gameObject.name}]");
            return;
        }

        if (materials == null)
        {
            Debug.LogWarning($"GhostTrail: material is null, go[{this.gameObject.name}]");            
            return;
        }

        if (materials.Count < meshRenderer.sharedMesh.subMeshCount)
        {
            Debug.LogWarning($"materials.Count[{materials.Count}] < meshRenderer.sharedMesh.subMeshCount[{meshRenderer.sharedMesh.subMeshCount}], go[{this.gameObject.name}]");
            return;
        }

        var ghost = GhostPool.instance.Get();
        ghost.createTime = Time.time;
        ghost.lifeTime = lifeTime;
        ghost.materials = materials;
        ghost.layer = meshLayerMask;
        meshRenderer.BakeMesh(ghost.mesh);
        ghost.matrix = Matrix4x4.TRS(meshRenderer.transform.position, 
            meshRenderer.transform.rotation, 
            meshRenderer.transform.localScale);

        m_Ghosts.Add(ghost);
    }

    private void BuildGhost()
    {
        if (!m_IsTrailing) return;

        if (m_Ghosts.Count >= GhostPool.instance.maxSize)
        {
            EndTrail();
            return;
        }

        if (m_LastCreatePosition == Vector3.positiveInfinity)
        {
            CreateSingleGhost();
            m_LastCreatePosition = this.transform.position;
            m_LastCreateTime = Time.time;
            return;
        }

        if ((Time.time - m_LastCreateTime > timeInterval) && 
            (Vector3.Distance(m_LastCreatePosition, this.transform.position) > distInterval))
        {
            CreateSingleGhost();
            m_LastCreatePosition = this.transform.position;
            m_LastCreateTime = Time.time;
        }
    }

    private void UpdateGhost()
    {
        float time = Time.time;
        for (int i = m_Ghosts.Count - 1; i >= 0; i--)
        {
            Ghost ghost = m_Ghosts[i];
            if (time - ghost.createTime > lifeTime)
            {
                m_Ghosts.RemoveAt(i);
                GhostPool.instance.Return(ghost);
            }
            else
            {
                ghost.Update();
            }
        }
    }
}
