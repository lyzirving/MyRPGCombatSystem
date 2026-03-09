using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

public class VFXManager : SingletonMono<VFXManager>
{
    private int m_Id;
    private Dictionary<string, AsyncOperationHandle<GameObject>> m_CachedPrefabs = new Dictionary<string, AsyncOperationHandle<GameObject>>();
    private Dictionary<string, ObjectPool<VFXInstance>> m_ObjectPools = new Dictionary<string, ObjectPool<VFXInstance>>();   

    #region Virtual Methods
    public override void OnInit()
    {
        m_Id = 0;
    }

    public override void OnDeInit()
    {
        foreach (var pool in m_ObjectPools)
        {
            pool.Value.Clear();
        }
        m_ObjectPools.Clear();

        foreach (var prefab in m_CachedPrefabs)
        {
            prefab.Value.Release();
        }
        m_CachedPrefabs.Clear();
    }
    #endregion

    public void Play(string vfxPath, Vector3 position, Quaternion rotation)
    {
        VFXInstance vfx = GetPooledObject(vfxPath);
        if (vfx == null) return;

        vfx.Play(position, rotation);
    }

    public void Release(VFXInstance vfx)
    {
        if(!m_ObjectPools.TryGetValue(vfx.key, out var pool)) return;
        pool.Release(vfx);
    }

    private VFXInstance GetPooledObject(string vfxPath)
    {
        if(m_ObjectPools.TryGetValue(vfxPath, out var pool))
            return pool.Get();

        if(!CheckVfx(vfxPath))
            return null;

        pool = new ObjectPool<VFXInstance>(
            createFunc: () =>
            {
                var prefab = GetPrefab(vfxPath);
                if (prefab == null) return null;
                var gameObject = Instantiate<GameObject>(prefab);
                gameObject.name = $"PooledVFX-{m_Id}";
                var component = gameObject.AddComponent<VFXInstance>();
                component.key = vfxPath;
                gameObject.SetActive(false);
                ++m_Id;
                return component;
            },
            actionOnGet: (vfx) => 
            {
                vfx?.gameObject.SetActive(true);
            }, 
            actionOnRelease: (vfx) =>
            {
                vfx?.Reset();
                vfx?.gameObject.SetActive(false);
            }, 
            actionOnDestroy: (vfx) =>
            { 
                if(vfx != null)
                    Destroy(vfx.gameObject);
            });

        m_ObjectPools.Add(vfxPath, pool);

        return pool.Get();
    }

    private GameObject GetPrefab(string vfxPath)
    {
        AsyncOperationHandle<GameObject> handle;
        if (m_CachedPrefabs.TryGetValue(vfxPath, out handle))
            return handle.Result;

        handle = Addressables.LoadAssetAsync<GameObject>(vfxPath);
        handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var components = handle.Result.GetComponentsInChildren<ParticleSystem>();
            if (components == null || components.Length == 0)
            {
                Debug.LogError($"err! prefab from[{vfxPath}] doesn't have any ParticleSystem");
                return null;
            }
            m_CachedPrefabs.Add(vfxPath, handle);
            return handle.Result;
        }
        else
        {
            Debug.LogError($"err! fail to load prefab from[{vfxPath}]");
            return null;
        }
    }

    private bool CheckVfx(string vfxPath)
    {
        AsyncOperationHandle<GameObject> handle;
        if (m_CachedPrefabs.TryGetValue(vfxPath, out handle))
            return true;

        handle = Addressables.LoadAssetAsync<GameObject>(vfxPath);
        handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var components = handle.Result.GetComponentsInChildren<ParticleSystem>();
            if (components == null || components.Length == 0)
            {
                Debug.LogError($"err! prefab from[{vfxPath}] doesn't have any ParticleSystem");
                return false;
            }
            m_CachedPrefabs.Add(vfxPath, handle);
            return true;
        }
        else
        {
            Debug.LogError($"err! invalid vfx prefab[{vfxPath}]");
            return false;
        }
    }
}
