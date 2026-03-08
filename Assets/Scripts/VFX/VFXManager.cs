using System.Collections.Generic;
using UnityEngine;

public class VFXManager : SingletonMono<VFXManager>
{
    private int m_Id;
    private Dictionary<string, VFXInstance> m_PooledObjects = new Dictionary<string, VFXInstance>();   

    #region Virtual Methods
    public override void OnInit()
    {
        m_Id = 0;
    }

    public override void OnDeInit()
    {
    }
    #endregion

    public void Play(string vfxPath, Vector3 position, Quaternion rotation, float time)
    {      
        // one prefab can be played in multiple places at one time, how to achieve that?
    }

    #region Pooled Methods
    private VFXInstance OnCreateInstance()
    { 
        return null;
    }

    private void OnGetInstance(VFXInstance instance)
    {        
    }

    private void OnReleaseInstance(VFXInstance instance)
    {
    }

    private void OnDestroyInstance(VFXInstance instance)
    {
        Destroy(instance);
    }
    #endregion
}
