using System.Collections.Generic;
using UnityEngine;

public class Ghost
{
    public float createTime;
    public float lifeTime;

    public Mesh mesh;
    public Matrix4x4 matrix;
    public int layer;
    public List<Material> materials;

    private MaterialPropertyBlock m_MaterialPropertyBlock;

    public Ghost()
    {
        mesh = new Mesh();
        m_MaterialPropertyBlock = new MaterialPropertyBlock();
    }

    public void Update()
    {
        if (materials.Count < mesh.subMeshCount)
        {
            Debug.LogError("Ghost: Check your subMesh materials!");
            return;
        }

        float ratio = Mathf.Clamp01((Time.time - createTime) / lifeTime);
        for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
        {
            float baseAlpha = materials[subMeshIndex].GetFloat("_Alpha");
            float alpha = baseAlpha - baseAlpha * ratio;
            m_MaterialPropertyBlock.SetFloat("_Alpha", alpha);
            Graphics.DrawMesh(mesh, matrix, materials[subMeshIndex], layer, null, subMeshIndex, m_MaterialPropertyBlock, false, false, false);
        }
    }
}
