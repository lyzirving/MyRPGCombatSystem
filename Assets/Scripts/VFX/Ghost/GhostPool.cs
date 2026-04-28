using System;
using UnityEngine;
using UnityEngine.Pool;

public class GhostPool : SingletonMono<GhostPool>
{
    [SerializeField] private bool m_CollectionCheck = true;
    [SerializeField] private int m_DefaultCapacity = 10;
    [SerializeField] private int m_MaxSize = 20;

    public int maxSize => m_MaxSize;

    private ObjectPool<Ghost> m_Pool;

    public override void OnInit()
    {
        m_Pool = new ObjectPool<Ghost>(OnCreateFromPool,
            OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject,
            m_CollectionCheck, m_DefaultCapacity, m_MaxSize);
    }

    public override void OnDeInit()
    {
        Clear();
    }

    #region Exposed API
    public Ghost Get()
    {
        return m_Pool.Get();
    }

    public void Return(Ghost ghost)
    { 
        m_Pool.Release(ghost);
    }

    public void Clear()
    { 
        m_Pool.Clear(); 
    }
    #endregion

    #region Pooling Methods
    // Invoked when creating an item to populate the object pool
    private Ghost OnCreateFromPool()
    {
        var item = new Ghost();
        return item;
    }

    // Invoked when retrieving the next item from the object pool
    private void OnGetFromPool(Ghost ghost)
    {
    }

    // Invoked when returning an item to the object pool
    private void OnReleaseToPool(Ghost ghost)
    {
        ghost.mesh?.Clear();
        ghost.materials = null;
    }

    // Invoked when the maximum number of pooled items is exceeded (i.e. destroy the pooled object)
    private void OnDestroyPooledObject(Ghost ghost)
    {
    }
    #endregion
}
